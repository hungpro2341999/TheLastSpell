import os
import re
import json
import xml.etree.ElementTree as ET
from http.server import HTTPServer, BaseHTTPRequestHandler
from urllib.parse import parse_qs, urlparse
import webbrowser

TEXTASSET_DIR = r"d:\Game_Complier\TextAsset"
PORT = 8080

def get_all_txt_files():
    if not os.path.exists(TEXTASSET_DIR):
        return []
    files = []
    for f in os.listdir(TEXTASSET_DIR):
        if f.endswith('.txt') and not f.endswith('.meta.txt'):
            files.append(f)
    return sorted(files)

def parse_enemy_file(filename):
    filepath = os.path.join(TEXTASSET_DIR, filename)
    if not os.path.exists(filepath):
        return []
    try:
        tree = ET.parse(filepath)
        root = tree.getroot()
        results = []
        for elem in root.findall('.//EnemyUnitTemplateDefinition') + root.findall('.//BossUnitTemplateDefinition') + root.findall('.//EliteEnemyUnitTemplateDefinition'):
            enemy_id = elem.attrib.get('Id', 'Unknown')
            health = elem.findtext('HealthTotal', 'N/A')
            armor = elem.findtext('ArmorTotal', '0')
            move = elem.findtext('MovePointsTotal', 'N/A')
            dodge = elem.findtext('Dodge', '0')
            block = elem.findtext('Block', '0')
            xp = elem.findtext('ExperienceGain', 'N/A')
            tier = elem.findtext('Tier', 'N/A')
            
            skills = [s.text for s in elem.findall('.//SkillToDisplay') if s.text]
            if not skills:
                damage_skill = elem.findtext('DamageSkillId')
                if damage_skill:
                    skills.append(damage_skill)

            goals = []
            for g in elem.findall('.//Goal'):
                g_id = g.attrib.get('Id', '')
                s_id = g.findtext('SkillId') or (g.find('SkillId').attrib.get('Value') if g.find('SkillId') is not None else '')
                targets = [t.tag for t in g.findall('.//TargetTypes/*')]
                goals.append({"id": g_id, "skill": s_id, "targets": targets})

            results.append({
                "id": enemy_id,
                "type": elem.tag.replace("TemplateDefinition", ""),
                "tier": tier,
                "health": health,
                "armor": armor,
                "move": move,
                "dodge": dodge,
                "block": block,
                "xp": xp,
                "skills": skills,
                "goals": goals,
                "file": filename
            })
        return results
    except Exception as e:
        print(f"Error parsing {filename}: {e}")
        return []

def parse_skill_file(filename):
    filepath = os.path.join(TEXTASSET_DIR, filename)
    if not os.path.exists(filepath):
        return []
    try:
        tree = ET.parse(filepath)
        root = tree.getroot()
        results = []
        for elem in root.findall('.//SkillDefinition'):
            skill_id = elem.attrib.get('Id', 'Unknown')
            cost_ap = elem.findtext('.//ActionPoints', '0')
            cost_mana = elem.findtext('.//Mana', '0')
            range_min = '0'
            range_max = '0'
            range_elem = elem.find('.//Range')
            if range_elem is not None:
                range_min = range_elem.attrib.get('Min', '0')
                range_max = range_elem.attrib.get('Max', '0')
            
            effects = [eff.tag for eff in elem.findall('.//SkillEffectDefinitions/*')]

            results.append({
                "id": skill_id,
                "file": filename,
                "cost_ap": cost_ap,
                "cost_mana": cost_mana,
                "range": f"{range_min}-{range_max}",
                "effects": effects
            })
        return results
    except Exception as e:
        return []

def search_text_files(query, category):
    files = get_all_txt_files()
    results = []
    query_lower = query.lower()

    if category == "enemies":
        enemy_files = [f for f in files if "EnemyUnitTemplate" in f or "BossUnitTemplate" in f or "EliteEnemyUnitTemplate" in f]
        for f in enemy_files:
            enemies = parse_enemy_file(f)
            for e in enemies:
                if not query or query_lower in e['id'].lower() or query_lower in e['type'].lower() or any(query_lower in s.lower() for s in e['skills']):
                    results.append(e)
    elif category == "skills":
        skill_files = [f for f in files if "SkillDefinitions" in f]
        for f in skill_files:
            skills = parse_skill_file(f)
            for s in skills:
                if not query or query_lower in s['id'].lower():
                    results.append(s)
    elif category == "all_files":
        for f in files:
            if not query or query_lower in f.lower():
                results.append({"filename": f, "size": os.path.getsize(os.path.join(TEXTASSET_DIR, f))})
    return results

HTML_TEMPLATE = """<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>The Last Spell - TextAsset Viewer</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">
    <style>
        body { background-color: #121214; color: #e1e1e6; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; }
        .navbar { background-color: #1a1a1e; border-bottom: 1px solid #29292e; }
        .card { background-color: #1e1e24; border: 1px solid #2e2e38; color: #e1e1e6; transition: transform 0.2s; }
        .card:hover { transform: translateY(-2px); border-color: #7b2cbf; }
        .badge-stat { background-color: #2a2a36; color: #a9a9b8; border: 1px solid #3a3a4c; }
        .badge-hp { background-color: #4a1525; color: #ff85a1; border: 1px solid #701c33; }
        .badge-armor { background-color: #1d3557; color: #a8dadc; border: 1px solid #457b9d; }
        .badge-move { background-color: #1b4332; color: #b7e4c7; border: 1px solid #2d6a4f; }
        .badge-skill { background-color: #3c096c; color: #e0aaff; border: 1px solid #5a189a; }
        .badge-boss { background-color: #7209b7; color: #f72585; font-weight: bold; }
        .badge-elite { background-color: #ffb703; color: #000; font-weight: bold; }
        .search-box { background-color: #1a1a20; border: 1px solid #333340; color: #fff; }
        .search-box:focus { background-color: #20202a; color: #fff; border-color: #9d4edd; box-shadow: none; }
        pre { background-color: #0d0d11; color: #a9b7c6; padding: 15px; border-radius: 8px; max-height: 550px; overflow: auto; border: 1px solid #22222e; }
        .nav-pills .nav-link { color: #a0a0b0; background-color: #18181f; margin-right: 8px; border: 1px solid #2a2a35; }
        .nav-pills .nav-link.active { background-color: #7b2cbf; color: #fff; }
        .modal-content { background-color: #18181e; color: #e1e1e6; border: 1px solid #333344; }
    </style>
</head>
<body>
    <nav class="navbar navbar-dark px-4 py-3">
        <div class="container-fluid">
            <span class="navbar-brand mb-0 h1 fs-4"><i class="bi bi-filetype-xml text-warning me-2"></i>The Last Spell — Asset Viewer</span>
            <span class="text-secondary small">TextAsset Path: <code>d:\\Game_Complier\\TextAsset</code></span>
        </div>
    </nav>

    <div class="container-fluid px-4 py-4">
        <div class="row mb-4">
            <div class="col-md-4 mb-2">
                <ul class="nav nav-pills" id="categoryTab">
                    <li class="nav-item"><a class="nav-link active" href="#" onclick="setCategory('enemies')"><i class="bi bi-bug me-1"></i>Enemies & Bosses</a></li>
                    <li class="nav-item"><a class="nav-link" href="#" onclick="setCategory('skills')"><i class="bi bi-magic me-1"></i>Skills</a></li>
                    <li class="nav-item"><a class="nav-link" href="#" onclick="setCategory('all_files')"><i class="bi bi-folder2-open me-1"></i>Files Browser</a></li>
                </ul>
            </div>
            <div class="col-md-8">
                <div class="input-group">
                    <span class="input-group-text search-box border-end-0"><i class="bi bi-search text-secondary"></i></span>
                    <input type="text" id="searchInput" class="form-control search-box border-start-0" placeholder="Tìm kiếm theo tên ID (ví dụ: Clawer, Harpy, Boomer...), loại skill, hoặc tên file..." onkeyup="handleSearch()">
                </div>
            </div>
        </div>

        <div id="resultsContainer" class="row g-3">
            <!-- Results rendered here -->
        </div>
    </div>

    <!-- Modal view Raw XML -->
    <div class="modal fade" id="fileModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-xl modal-dialog-scrollable">
            <div class="modal-content">
                <div class="modal-header border-secondary">
                    <h5 class="modal-title" id="modalTitle">File Details</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <pre><code id="modalContent">Loading...</code></pre>
                </div>
            </div>
        </div>
    </div>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/bootstrap.bundle.min.js"></script>
    <script>
        let currentCategory = 'enemies';
        let searchTimeout = null;

        function setCategory(cat) {
            currentCategory = cat;
            document.querySelectorAll('#categoryTab .nav-link').forEach(el => el.classList.remove('active'));
            event.target.classList.add('active');
            loadResults();
        }

        function handleSearch() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(loadResults, 300);
        }

        function loadResults() {
            const query = document.getElementById('searchInput').value;
            const container = document.getElementById('resultsContainer');
            container.innerHTML = '<div class="col-12 text-center py-5 text-secondary"><div class="spinner-border spinner-border-sm me-2"></div>Đang tải dữ liệu định nghĩa...</div>';

            fetch(`/api/search?cat=${currentCategory}&q=${encodeURIComponent(query)}`)
                .then(res => res.json())
                .then(data => renderResults(data));
        }

        function renderResults(data) {
            const container = document.getElementById('resultsContainer');
            if (!data || data.length === 0) {
                container.innerHTML = '<div class="col-12 text-center py-5 text-secondary"><i class="bi bi-inbox fs-1 d-block mb-2"></i>Không tìm thấy dữ liệu phù hợp.</div>';
                return;
            }

            let html = '';
            if (currentCategory === 'enemies') {
                data.forEach(e => {
                    const isBoss = e.type.includes('Boss');
                    const isElite = e.type.includes('Elite');
                    const typeBadge = isBoss ? '<span class="badge badge-boss me-1">BOSS</span>' : (isElite ? '<span class="badge badge-elite me-1">ELITE</span>' : '<span class="badge bg-secondary me-1">Enemy</span>');

                    let skillsHtml = e.skills.map(s => `<span class="badge badge-skill me-1 mb-1">${s}</span>`).join('');
                    let goalsHtml = e.goals.map(g => `<div class="small text-secondary">• <strong>${g.id}</strong>: Skill <code>${g.skill}</code> → ${g.targets.join(', ')}</div>`).join('');

                    html += `
                        <div class="col-md-6 col-lg-4">
                            <div class="card h-100 p-3">
                                <div class="d-flex justify-content-between align-items-start mb-2">
                                    <div>
                                        ${typeBadge}
                                        <h5 class="card-title d-inline text-warning">${e.id}</h5>
                                    </div>
                                    <span class="badge badge-stat">Tier ${e.tier}</span>
                                </div>
                                <div class="mb-3">
                                    <span class="badge badge-hp me-1"><i class="bi bi-heart-fill me-1"></i>HP: ${e.health}</span>
                                    <span class="badge badge-armor me-1"><i class="bi bi-shield-fill me-1"></i>Armor: ${e.armor}</span>
                                    <span class="badge badge-move me-1"><i class="bi bi-boot-fill me-1"></i>Move: ${e.move}</span>
                                    <span class="badge badge-stat me-1">Dodge: ${e.dodge}%</span>
                                    <span class="badge badge-stat">XP: ${e.xp}</span>
                                </div>
                                ${skillsHtml ? `<div class="mb-2"><strong class="small text-secondary d-block mb-1">Kỹ năng:</strong>${skillsHtml}</div>` : ''}
                                ${goalsHtml ? `<div class="mb-2"><strong class="small text-secondary d-block mb-1">Mục tiêu AI (Goals):</strong>${goalsHtml}</div>` : ''}
                                <div class="mt-auto pt-2 border-top border-secondary-subtle d-flex justify-content-between align-items-center">
                                    <small class="text-secondary opacity-75">${e.file}</small>
                                    <button class="btn btn-sm btn-outline-purple" style="color: #c77dff; border-color: #7b2cbf;" onclick="viewRaw('${e.file}', '${e.id}')"><i class="bi bi-code-slash me-1"></i>Xem XML</button>
                                </div>
                            </div>
                        </div>
                    `;
                });
            } else if (currentCategory === 'skills') {
                data.forEach(s => {
                    let effHtml = s.effects.map(eff => `<span class="badge badge-skill me-1 mb-1">${eff}</span>`).join('');
                    html += `
                        <div class="col-md-6 col-lg-4">
                            <div class="card h-100 p-3">
                                <h5 class="card-title text-info mb-2"><i class="bi bi-magic me-2"></i>${s.id}</h5>
                                <div class="mb-2">
                                    <span class="badge badge-stat me-1">AP: ${s.cost_ap}</span>
                                    <span class="badge badge-stat me-1">Mana: ${s.cost_mana}</span>
                                    <span class="badge badge-stat me-1">Range: ${s.range}</span>
                                </div>
                                ${effHtml ? `<div class="mb-2">${effHtml}</div>` : ''}
                                <div class="mt-auto pt-2 border-top border-secondary-subtle d-flex justify-content-between align-items-center">
                                    <small class="text-secondary opacity-75">${s.file}</small>
                                    <button class="btn btn-sm btn-outline-info" onclick="viewRaw('${s.file}', '${s.id}')"><i class="bi bi-code-slash me-1"></i>Xem XML</button>
                                </div>
                            </div>
                        </div>
                    `;
                });
            } else if (currentCategory === 'all_files') {
                data.forEach(f => {
                    const kb = (f.size / 1024).toFixed(1);
                    html += `
                        <div class="col-md-6 col-lg-4">
                            <div class="card p-3">
                                <div class="d-flex justify-content-between align-items-center">
                                    <div>
                                        <i class="bi bi-file-text text-warning me-2 fs-5"></i>
                                        <strong class="text-light">${f.filename}</strong>
                                        <div class="small text-secondary">${kb} KB</div>
                                    </div>
                                    <button class="btn btn-sm btn-outline-secondary" onclick="viewRaw('${f.filename}')">Mở File</button>
                                </div>
                            </div>
                        </div>
                    `;
                });
            }
            container.innerHTML = html;
        }

        function viewRaw(filename, targetId = '') {
            document.getElementById('modalTitle').innerText = filename + (targetId ? ` [${targetId}]` : '');
            document.getElementById('modalContent').innerText = 'Đang tải nội dung file...';
            const modal = new bootstrap.Modal(document.getElementById('fileModal'));
            modal.show();

            fetch(`/api/raw?file=${encodeURIComponent(filename)}`)
                .then(res => res.text())
                .then(text => {
                    document.getElementById('modalContent').innerText = text;
                });
        }

        // Initial load
        loadResults();
    </script>
</body>
</html>
"""

class RequestHandler(BaseHTTPRequestHandler):
    def do_GET(self):
        parsed = urlparse(self.path)
        if parsed.path == '/' or parsed.path == '/index.html':
            self.send_response(200)
            self.send_header('Content-type', 'text/html; charset=utf-8')
            self.end_headers()
            self.wfile.write(HTML_TEMPLATE.encode('utf-8'))
        elif parsed.path == '/api/search':
            qs = parse_qs(parsed.query)
            cat = qs.get('cat', ['enemies'])[0]
            query = qs.get('q', [''])[0]
            results = search_text_files(query, cat)
            
            self.send_response(200)
            self.send_header('Content-type', 'application/json; charset=utf-8')
            self.end_headers()
            self.wfile.write(json.dumps(results).encode('utf-8'))
        elif parsed.path == '/api/raw':
            qs = parse_qs(parsed.query)
            filename = qs.get('file', [''])[0]
            filepath = os.path.join(TEXTASSET_DIR, filename)
            if os.path.exists(filepath) and filename.endswith('.txt'):
                self.send_response(200)
                self.send_header('Content-type', 'text/plain; charset=utf-8')
                self.end_headers()
                with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                    self.wfile.write(f.read().encode('utf-8'))
            else:
                self.send_response(404)
                self.end_headers()
        else:
            self.send_response(404)
            self.end_headers()

    def log_message(self, format, *args):
        return

def main():
    server = HTTPServer(('localhost', PORT), RequestHandler)
    url = f"http://localhost:{PORT}"
    print(f"=================================================")
    print(f"  The Last Spell — TextAsset Web Viewer")
    print(f"  Running at: {url}")
    print(f"  Press Ctrl+C to stop the server.")
    print(f"=================================================")
    webbrowser.open(url)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nServer stopped.")

if __name__ == '__main__':
    main()
