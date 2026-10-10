# Codebase Atlas

Offline viewer for script relationships in The Last Spell. Open `CodebaseDashboard\index.html` in a browser. The module matrix shows dependency counts by source and target module; selecting a script shows inheritance/interface implementation, incoming and outgoing file relationships, and available source references. Click a parent or child type to navigate to its script, or “Khai báo” to see the declaration.

Regenerate after source changes from the project root:

```powershell
py -3 tools\codebase_dashboard.py
```

Use `--watch` to regenerate when script files change. The page itself does not reload automatically; refresh the browser tab after regeneration.

The analyzer uses standard Python only. C# usage links are inferred from locally declared type names in a file's namespace or imported internal namespaces. Inheritance links are parsed separately from C# type declarations and resolved to a unique internal type when possible; external or ambiguous bases stay visible as unresolved. Python links use import statements. Batch links use referenced script paths. The graph is an approximation, not a compiler's complete call graph; decision points are a lightweight syntax count.
