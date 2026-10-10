#!/usr/bin/env python3
"""Build a local script-dependency atlas for The Last Spell."""
from __future__ import annotations

import argparse
import ast
import hashlib
import html
import importlib.util
import json
import os
import re
import subprocess
import sys
import time
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path

SKIP_DIRS = {".git", ".vs", "bin", "obj", "packages", "CodebaseDashboard", "node_modules"}
GENERATED_SUFFIXES = (".g.cs", ".generated.cs", ".designer.cs")
SCRIPT_LANGUAGES = {".cs": "C#", ".py": "Python", ".pyw": "Python", ".bat": "Batch", ".cmd": "Batch"}
NAMESPACE_RE = re.compile(r"^\s*namespace\s+([\w.]+)", re.MULTILINE)
USING_RE = re.compile(r"^\s*(?:global\s+)?using\s+([\w.]+)\s*;", re.MULTILINE)
DECL_RE = re.compile(r"\b(?:class|struct|interface|enum|record)\s+(\w+)")
TYPE_HEADER_RE = re.compile(
    r"\b(?P<kind>class|interface|struct|record)\s+(?P<name>[A-Za-z_]\w*)"
    r"(?:\s*<[^{};]+?>)?\s*:\s*(?P<bases>[^{};]+?)(?=\s*(?:\bwhere\b|\{|;))",
    re.MULTILINE,
)
DECL_KIND_RE = re.compile(r"\b(?P<kind>class|struct|interface|enum|record)\s+(?P<name>\w+)")
DECISION_RE = re.compile(r"\b(?:if|for|foreach|while|case|catch)\b|&&|\|\||\?(?![.?])")


def mask_comments(text: str) -> tuple[str, list[str]]:
    """Replace comments with spaces while preserving strings and line positions."""
    out = list(text)
    lines = text.splitlines()
    i, n = 0, len(text)
    mode = "code"
    raw_end = 0
    while i < n:
        if mode == "code":
            if text.startswith("//", i):
                j = i
                while j < n and text[j] not in "\r\n":
                    out[j] = " "
                    j += 1
                i = j
                continue
            if text.startswith("/*", i):
                out[i] = out[i + 1] = " "
                i += 2
                mode = "block"
                continue
            # Raw strings (including interpolated raw strings) use three or more quotes.
            if text[i] == '"':
                q = i
                while q < n and text[q] == '"':
                    q += 1
                if q - i >= 3:
                    raw_end = q - i
                    mode = "raw"
                    i = q
                    continue
                mode = "string"
            elif text[i] == "'":
                mode = "char"
            i += 1
            continue
        if mode == "block":
            if text.startswith("*/", i):
                out[i] = out[i + 1] = " "
                i += 2
                mode = "code"
            else:
                if text[i] not in "\r\n":
                    out[i] = " "
                i += 1
            continue
        if mode in ("string", "char"):
            quote = '"' if mode == "string" else "'"
            if text[i] == "\\":
                i += 2
            elif text[i] == quote:
                mode = "code"
                i += 1
            else:
                i += 1
            continue
        if mode == "raw":
            if text[i] == '"' and len(text[i:]) >= raw_end and text[i:i + raw_end] == '"' * raw_end:
                i += raw_end
                mode = "code"
            else:
                i += 1
    return "".join(out), lines


def git_info(root: Path) -> dict[str, str]:
    def git(*args: str) -> str:
        try:
            return subprocess.run(["git", "-C", str(root), *args], capture_output=True,
                                  text=True, timeout=3, check=False).stdout.strip()
        except (OSError, subprocess.TimeoutExpired):
            return ""
    return {"commit": git("rev-parse", "--short", "HEAD"),
            "branch": git("branch", "--show-current")}


def source_files(root: Path):
    for current, dirs, files in os.walk(root):
        dirs[:] = sorted(d for d in dirs if d not in SKIP_DIRS and not d.startswith("."))
        for name in sorted(files):
            if not name.lower().endswith(".cs") or name.lower().endswith(GENERATED_SUFFIXES):
                continue
            yield Path(current, name)


def all_script_files(root: Path):
    for current, dirs, names in os.walk(root):
        dirs[:] = sorted(d for d in dirs if d not in SKIP_DIRS and not d.startswith("."))
        for name in sorted(names):
            path = Path(current, name)
            if path.suffix.lower() in SCRIPT_LANGUAGES:
                if path.suffix.lower() == ".cs" and name.lower().endswith(GENERATED_SUFFIXES):
                    continue
                yield path


def module_name(namespace: str) -> str:
    parts = namespace.split(".") if namespace else ["(global namespace)"]
    if len(parts) >= 2 and parts[0] == "TheLastStand":
        return ".".join(parts[:2])
    return parts[0]


def split_base_types(value: str) -> list[str]:
    """Split a C# base list without splitting generic type arguments."""
    parts, depth, start = [], 0, 0
    for index, char in enumerate(value):
        if char == "<":
            depth += 1
        elif char == ">":
            depth = max(0, depth - 1)
        elif char == "," and depth == 0:
            parts.append(value[start:index].strip())
            start = index + 1
    parts.append(value[start:].strip())
    return [part for part in parts if part]


def analyze(root: Path) -> dict:
    files = []
    source_hash = hashlib.sha256()
    script_hash = hashlib.sha256()
    ns_to_files: dict[str, list[dict]] = defaultdict(list)
    module_stats: dict[str, dict] = {}
    imports: set[tuple[str, str]] = set()
    ns_names: set[str] = set()
    type_index: dict[str, list[tuple[str, str]]] = defaultdict(list)
    type_declarations: dict[str, list[dict]] = defaultdict(list)
    inheritance_headers: list[dict] = []
    clean_by_path: dict[str, str] = {}
    raw_by_path: dict[str, str] = {}
    declared_by_path: dict[str, set[str]] = {}
    imports_by_path: dict[str, set[str]] = defaultdict(set)
    pending = []

    for path in source_files(root):
        rel = path.relative_to(root).as_posix()
        try:
            raw = path.read_text(encoding="utf-8-sig", errors="replace")
        except OSError:
            continue
        source_hash.update(rel.encode("utf-8", errors="replace"))
        source_hash.update(b"\0")
        source_hash.update(raw.encode("utf-8", errors="replace"))
        clean, original_lines = mask_comments(raw)
        namespace_match = NAMESPACE_RE.search(clean)
        namespace = namespace_match.group(1) if namespace_match else "(global namespace)"
        ns_names.add(namespace)
        lines = clean.splitlines()
        physical = len(original_lines)
        blank = sum(not line.strip() for line in lines)
        code = physical - blank
        decisions = len(DECISION_RE.findall(clean))
        declared_types = DECL_RE.findall(clean)
        declarations = len(declared_types)
        info = {"path": rel, "namespace": namespace, "module": module_name(namespace),
                "lines": physical, "code": code, "blank": blank,
                "decisions": decisions, "declarations": declarations}
        files.append(info)
        ns_to_files[namespace].append(info)
        clean_by_path[rel] = clean
        raw_by_path[rel] = raw
        declared_by_path[rel] = set(declared_types)
        for declared_type in declared_types:
            type_index[declared_type].append((rel, namespace))
        for match in DECL_KIND_RE.finditer(clean):
            type_declarations[match.group("name")].append({
                "path": rel, "namespace": namespace, "kind": match.group("kind")})
        for match in TYPE_HEADER_RE.finditer(clean):
            inheritance_headers.append({
                "path": rel, "namespace": namespace, "kind": match.group("kind"),
                "name": match.group("name"), "line": clean.count("\n", 0, match.start()) + 1,
                "bases": split_base_types(match.group("bases")),
                "snippet": " ".join(raw[match.start():match.end()].split())[:240],
            })
        pending.append((namespace, clean, info, set(declared_types)))

    for namespace, clean, info, declared_types in pending:
        source_module = module_name(namespace)
        for imported in USING_RE.findall(clean):
            # A using is internal if it equals or contains a namespace declared in this project.
            candidates = [known for known in ns_names
                          if known == imported or known.startswith(imported + ".")]
            for known in candidates:
                imports_by_path[info["path"]].add(known)
                target_module = module_name(known)
                if target_module != source_module:
                    imports.add((source_module, target_module))

    inheritance_edges = []
    for header in inheritance_headers:
        for base in header["bases"]:
            base_type = re.sub(r"<.*", "", base).strip().replace("global::", "")
            base_symbol = base_type.rsplit(".", 1)[-1]
            explicit_namespace = base_type.rsplit(".", 1)[0] if "." in base_type else None
            allowed = imports_by_path[header["path"]] | {header["namespace"]}
            namespace_parts = header["namespace"].split(".")
            allowed.update(".".join(namespace_parts[:i]) for i in range(1, len(namespace_parts)))
            candidates = [item for item in type_declarations.get(base_symbol, ())
                          if item["namespace"] == explicit_namespace] if explicit_namespace else [
                item for item in type_declarations.get(base_symbol, ()) if item["namespace"] in allowed]
            # Ambiguous symbols remain unlinked instead of claiming a false project relationship.
            target = candidates[0] if len(candidates) == 1 else None
            if target and target["kind"] == "interface":
                relation = "extends-interface" if header["kind"] == "interface" else "implements"
            elif target:
                relation = "extends"
            else:
                relation = "unresolved"
            inheritance_edges.append({
                "from": header["path"], "to": target["path"] if target else None,
                "sourceType": header["name"], "sourceKind": header["kind"],
                "baseType": base, "targetKind": target["kind"] if target else None,
                "relation": relation, "line": header["line"], "snippet": header["snippet"],
            })

    script_items = {}
    file_edges: set[tuple[str, str]] = set()
    file_edge_details: dict[tuple[str, str], set[str]] = defaultdict(set)
    file_edge_references: dict[tuple[str, str], list[dict]] = defaultdict(list)
    for f in files:
        path, namespace = f["path"], f["namespace"]
        script_items[path] = {"path": path, "language": "C#", "module": f["module"],
                              "namespace": namespace, "lines": f["lines"], "code": f["code"],
                              "decisions": f["decisions"]}
        allowed_namespaces = imports_by_path[path] | {namespace}
        tokens = set(re.findall(r"\b[A-Za-z_]\w*\b", clean_by_path[path])) - declared_by_path[path]
        # Resolve declared project types in the file's own or explicitly imported namespaces.
        for symbol in tokens:
            for target_path, target_namespace in type_index.get(symbol, ()):
                if target_path != path and target_namespace in allowed_namespaces:
                    file_edges.add((path, target_path))
                    file_edge_details[(path, target_path)].add(f"Kiểu được tham chiếu: {symbol} ({target_namespace})")
                    key = (path, target_path)
                    if len(file_edge_references[key]) < 30:
                        source_lines = clean_by_path[path].splitlines()
                        for line_no, source_line in enumerate(source_lines, 1):
                            if re.search(rf"\b{re.escape(symbol)}\b", source_line):
                                target_line = next((i for i, line in enumerate(clean_by_path[target_path].splitlines(), 1)
                                                    if re.search(rf"\b{re.escape(symbol)}\b", line)), 1)
                                file_edge_references[key].append({"symbol": symbol, "sourceLine": line_no,
                                    "targetLine": target_line,
                                    "snippet": raw_by_path[path].splitlines()[line_no - 1].strip()[:240]})
                                break

    non_csharp = []
    for path in all_script_files(root):
        if path.suffix.lower() == ".cs":
            continue
        rel = path.relative_to(root).as_posix()
        try:
            raw = path.read_text(encoding="utf-8-sig", errors="replace")
        except OSError:
            continue
        script_hash.update(rel.encode("utf-8", errors="replace"))
        script_hash.update(b"\0")
        script_hash.update(raw.encode("utf-8", errors="replace"))
        lines = raw.splitlines()
        item = {"path": rel, "language": SCRIPT_LANGUAGES[path.suffix.lower()],
                "module": path.parent.relative_to(root).as_posix() if path.parent != root else "(root)",
                "namespace": "", "lines": len(lines), "code": sum(bool(x.strip()) for x in lines),
                "decisions": len(DECISION_RE.findall(raw))}
        script_items[rel] = item
        non_csharp.append((path, rel, raw))

    module_by_import = defaultdict(list)
    for path, _, _ in non_csharp:
        module = path.relative_to(root).with_suffix("").as_posix().replace("/", ".")
        module_by_import[module].append(path.relative_to(root).as_posix())
        module_by_import[path.stem].append(path.relative_to(root).as_posix())
    for path, rel, raw in non_csharp:
        if path.suffix.lower() in {".py", ".pyw"}:
            try:
                tree = ast.parse(raw, filename=rel)
            except SyntaxError:
                continue
            module = path.relative_to(root).with_suffix("").as_posix().replace("/", ".")
            package = module.rpartition(".")[0]
            for node in ast.walk(tree):
                imported_names = []
                if isinstance(node, ast.Import):
                    imported_names = [alias.name for alias in node.names]
                elif isinstance(node, ast.ImportFrom):
                    name = node.module or ""
                    if node.level:
                        try:
                            name = importlib.util.resolve_name("." * node.level + name, package)
                        except (ImportError, ValueError):
                            name = ""
                    imported_names = [name] if name else []
                for imported in imported_names:
                    candidates = module_by_import.get(imported, [])
                    if not candidates:
                        parts = imported.split(".")
                        for end in range(len(parts) - 1, 0, -1):
                            candidates = module_by_import.get(".".join(parts[:end]), [])
                            if candidates:
                                break
                    for target in candidates:
                        if target != rel:
                            file_edges.add((rel, target))
                            file_edge_details[(rel, target)].add(f"Python import: {imported}")
                            key = (rel, target)
                            if len(file_edge_references[key]) < 30:
                                source_line = raw.splitlines()[node.lineno - 1].strip() if node.lineno <= len(raw.splitlines()) else imported
                                file_edge_references[key].append({"symbol": imported, "sourceLine": node.lineno,
                                    "targetLine": 1, "snippet": source_line[:240]})
        elif path.suffix.lower() in {".bat", ".cmd"}:
            lowered = raw.lower().replace("\\", "/")
            for target_path in script_items:
                if target_path == rel or target_path.lower() == rel.lower():
                    continue
                if Path(target_path).suffix.lower() in {".py", ".pyw", ".bat", ".cmd", ".ps1"}:
                    target_name = target_path.lower().replace("\\", "/")
                    if target_name in lowered or Path(target_name).name in lowered:
                        file_edges.add((rel, target_path))
                        file_edge_details[(rel, target_path)].add(f"Batch tham chiếu đường dẫn: {Path(target_path).name}")

    for rel, item in script_items.items():
        script_hash.update(rel.encode("utf-8", errors="replace"))
        script_hash.update(b"\0")
        script_hash.update(item["language"].encode("utf-8"))

    for f in files:
        stat = module_stats.setdefault(f["module"], {"name": f["module"], "files": 0,
                  "lines": 0, "code": 0, "decisions": 0, "declarations": 0})
        for key in ("files", "lines", "code", "decisions", "declarations"):
            stat[key] += f["lines"] > -1 if key == "files" else f[key]

    ns_stats = []
    for ns, group in ns_to_files.items():
        ns_stats.append({"name": ns, "module": module_name(ns),
                         "files": len(group), "lines": sum(x["lines"] for x in group),
                         "code": sum(x["code"] for x in group),
                         "decisions": sum(x["decisions"] for x in group),
                         "declarations": sum(x["declarations"] for x in group),
                         "largestFile": max(group, key=lambda x: x["lines"])["lines"]})

    graph_nodes = sorted(module_stats.values(), key=lambda x: (-x["code"], x["name"]))
    graph_edges = [{"from": a, "to": b} for a, b in sorted(imports)]
    largest_files = sorted(files, key=lambda x: (-x["lines"], x["path"]))[:30]
    hotspots = sorted(ns_stats, key=lambda x: (-x["decisions"], -x["code"]))[:30]
    return {"root": str(root), "generatedAt": datetime.now(timezone.utc).isoformat(),
            "sourceFingerprint": source_hash.hexdigest(),
            "scriptFingerprint": hashlib.sha256(source_hash.digest() + script_hash.digest()).hexdigest(),
            **git_info(root),
            "summary": {"files": len(files), "lines": sum(x["lines"] for x in files),
                        "code": sum(x["code"] for x in files),
                        "blank": sum(x["blank"] for x in files),
                        "decisions": sum(x["decisions"] for x in files),
                        "declarations": sum(x["declarations"] for x in files),
                        "namespaces": len(ns_stats), "modules": len(module_stats),
                        "dependencies": len(graph_edges), "scripts": len(script_items),
                        "scriptDependencies": len(file_edges),
                        "inheritanceLinks": len(inheritance_edges),
                        "internalInheritanceLinks": sum(bool(item["to"]) for item in inheritance_edges)},
            "modules": graph_nodes, "edges": graph_edges,
            "scripts": sorted(script_items.values(), key=lambda x: x["path"].lower()),
            "fileEdges": [{"from": a, "to": b, "details": sorted(file_edge_details[(a, b)]),
                           "references": file_edge_references[(a, b)]}
                          for a, b in sorted(file_edges)],
            "inheritanceEdges": inheritance_edges,
            "namespaces": sorted(ns_stats, key=lambda x: (-x["code"], x["name"])),
            "largestFiles": largest_files, "hotspots": hotspots}


def write_report(root: Path, data: dict) -> Path:
    out = root / "CodebaseDashboard"
    out.mkdir(parents=True, exist_ok=True)
    payload = {
        "generatedAt": data["generatedAt"],
        "branch": data["branch"],
        "commit": data["commit"],
        "summary": data["summary"],
        "scripts": data["scripts"],
        "edges": data["fileEdges"],
        "inheritance": data["inheritanceEdges"],
    }
    encoded = json.dumps(payload, ensure_ascii=False, separators=(",", ":")).replace("</", "<\\/")
    template = (out / "template.html").read_text(encoding="utf-8")
    (out / "index.html").write_text(template.replace("__DATA__", encoded), encoding="utf-8")
    (out / "latest.json").write_text(json.dumps({
        "generatedAt": data["generatedAt"],
        "scripts": data["summary"]["scripts"],
        "dependencies": data["summary"]["scriptDependencies"],
        "inheritanceLinks": data["summary"]["inheritanceLinks"],
    }, ensure_ascii=False, indent=2), encoding="utf-8")
    return out / "index.html"


def main() -> int:
    parser = argparse.ArgumentParser(description="Build the local Codebase Atlas viewer")
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1], help="Project root")
    parser.add_argument("--watch", action="store_true", help="Regenerate when script source changes")
    parser.add_argument("--interval", type=int, default=15, help="Watch interval in seconds")
    args = parser.parse_args()
    root = args.root.resolve()
    if not root.is_dir():
        parser.error(f"Project root does not exist: {root}")
    data = analyze(root)
    report = write_report(root, data)
    print(f"Updated {report}")
    print(f"{data['summary']['scripts']:,} scripts · {data['summary']['scriptDependencies']:,} dependencies")
    if args.watch:
        print(f"Watching {root} every {max(3, args.interval)} seconds. Press Ctrl+C to stop.")
        fingerprint = data["scriptFingerprint"]
        try:
            while True:
                time.sleep(max(3, args.interval))
                data = analyze(root)
                if data["scriptFingerprint"] != fingerprint:
                    fingerprint = data["scriptFingerprint"]
                    write_report(root, data)
                    print(f"Updated at {data['generatedAt']}")
        except KeyboardInterrupt:
            print("Stopped watching.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
