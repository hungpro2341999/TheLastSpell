(() => {
  const report = JSON.parse(document.querySelector('#reportData').textContent);
  const scripts = report.scripts;
  const edges = report.edges;
  const inheritance = report.inheritance || [];
  const fmt = value => Number(value || 0).toLocaleString('vi-VN');
  const byPath = new Map(scripts.map(script => [script.path, script]));
  const incoming = new Map(scripts.map(script => [script.path, []]));
  const outgoing = new Map(scripts.map(script => [script.path, []]));
  const parents = new Map(scripts.map(script => [script.path, []]));
  const children = new Map(scripts.map(script => [script.path, []]));
  const moduleStats = new Map();
  const matrix = new Map();
  let crossEdges = 0;
  let chosenModule = null;
  let chosenPair = null;
  let chosenScript = null;

  const el = (tag, className, text) => {
    const node = document.createElement(tag);
    if (className) node.className = className;
    if (text !== undefined) node.textContent = text;
    return node;
  };
  const nameOf = path => path.split('/').at(-1);
  const shortModule = module => module.replace(/^TheLastStand\./, '');
  const degree = path => (incoming.get(path)?.length || 0) + (outgoing.get(path)?.length || 0);
  const matrixKey = (from, to) => `${from}\u0000${to}`;

  for (const script of scripts) {
    if (!moduleStats.has(script.module)) moduleStats.set(script.module, {name: script.module, scripts: 0, incoming: 0, outgoing: 0, externalIn: 0, externalOut: 0, code: 0, decisions: 0});
    const item = moduleStats.get(script.module);
    item.scripts++;
    item.code += script.code || 0;
    item.decisions += script.decisions || 0;
  }
  for (const edge of edges) {
    const from = byPath.get(edge.from);
    const to = byPath.get(edge.to);
    if (!from || !to) continue;
    outgoing.get(edge.from).push(edge);
    incoming.get(edge.to).push(edge);
    moduleStats.get(from.module).outgoing++;
    moduleStats.get(to.module).incoming++;
    if (from.module !== to.module) {
      crossEdges++;
      moduleStats.get(from.module).externalOut++;
      moduleStats.get(to.module).externalIn++;
    }
    const key = matrixKey(from.module, to.module);
    matrix.set(key, (matrix.get(key) || 0) + 1);
  }
  for (const relation of inheritance) {
    parents.get(relation.from)?.push(relation);
    if (relation.to) children.get(relation.to)?.push(relation);
  }
  const modules = [...moduleStats.values()].sort((a, b) => b.scripts - a.scripts || a.name.localeCompare(b.name));
  const maxCell = Math.max(1, ...matrix.values());
  const sortedScripts = [...scripts].sort((a, b) => degree(b.path) - degree(a.path) || a.path.localeCompare(b.path));

  document.querySelector('#stamp').textContent = `Quét lần cuối\n${new Date(report.generatedAt).toLocaleString('vi-VN')}`;
  document.querySelector('#totalEdges').textContent = fmt(edges.length);
  document.querySelector('#totalScripts').textContent = fmt(scripts.length);
  document.querySelector('#totalModules').textContent = fmt(modules.length);
  document.querySelector('#crossEdges').textContent = fmt(crossEdges);
  document.querySelector('#densityText').textContent = `${(edges.length / Math.max(1, scripts.length)).toFixed(1)} liên kết đi ra trên mỗi script, tính trung bình`;
  const crossPercent = Math.round(crossEdges / Math.max(1, edges.length) * 100);
  const largest = modules[0];
  document.querySelector('#overviewExplain').textContent = `${crossPercent}% liên kết đi qua ranh giới module. ${shortModule(largest.name)} là module lớn nhất với ${fmt(largest.scripts)} script (${Math.round(largest.scripts / scripts.length * 100)}% project). Các vùng dày liên kết có thể cần kiểm tra tác động rộng hơn trước khi sửa.`;

  function statLine(label, value) {
    const line = el('div', 'statline');
    line.append(el('span', '', label), el('b', '', fmt(value)));
    return line;
  }

  function renderModuleDetail() {
    const host = document.querySelector('#moduleDetail');
    host.replaceChildren();
    if (chosenPair) {
      const [fromName, toName] = chosenPair;
      const matching = edges.filter(edge => byPath.get(edge.from)?.module === fromName && byPath.get(edge.to)?.module === toName);
      const sourceFiles = new Set(matching.map(edge => edge.from));
      const targetFiles = new Set(matching.map(edge => edge.to));
      host.append(el('div', 'module-type', 'Hướng phụ thuộc'), el('h3', '', `${shortModule(fromName)} → ${shortModule(toName)}`));
      host.append(statLine('Liên kết script', matching.length), statLine('Script nguồn', sourceFiles.size), statLine('Script đích', targetFiles.size));
      const insight = el('div', 'insight');
      insight.append(el('strong', '', 'Ý nghĩa'), el('span', '', `${shortModule(fromName)} dùng kiểu hoặc import từ ${shortModule(toName)}. Chọn một script bên dưới để xem tham chiếu cụ thể.`));
      host.append(insight, el('p', 'hint', 'Ma trận đếm cạnh giữa file. Một cặp file chỉ tính một liên kết dù có nhiều dòng tham chiếu.'));
      return;
    }
    const item = moduleStats.get(chosenModule) || modules[0];
    if (!item) return;
    host.append(el('div', 'module-type', 'Module đang xem'), el('h3', '', shortModule(item.name)));
    host.append(statLine('Script', item.scripts), statLine('Liên kết đi ra', item.outgoing), statLine('Liên kết đi vào', item.incoming), statLine('Đi ra module khác', item.externalOut));
    const insight = el('div', 'insight');
    const externalShare = Math.round(item.externalOut / Math.max(1, item.outgoing) * 100);
    insight.append(el('strong', '', 'Đọc mức kết nối'), el('span', '', `${externalShare}% liên kết đi ra của module này trỏ sang module khác. ${fmt(item.incoming)} liên kết đi vào cho biết có bao nhiêu cặp script khác đang dùng code tại đây.`));
    host.append(insight, el('p', 'hint', 'Bấm ô trong ma trận để xem chính xác hai module đang nối với nhau.'));
  }

  function renderMatrix() {
    const table = document.querySelector('#moduleMatrix');
    const head = el('thead');
    const headerRow = el('tr');
    const corner = el('th', 'corner', 'Dùng ↓  /  Được dùng →');
    corner.scope = 'col';
    headerRow.append(corner);
    for (const target of modules) {
      const th = el('th', 'col');
      th.scope = 'col';
      th.title = target.name;
      th.append(el('span', '', shortModule(target.name)));
      headerRow.append(th);
    }
    head.append(headerRow);
    const body = el('tbody');
    for (const source of modules) {
      const row = el('tr');
      const label = el('th', '', shortModule(source.name));
      label.scope = 'row';
      label.title = source.name;
      row.append(label);
      for (const target of modules) {
        const count = matrix.get(matrixKey(source.name, target.name)) || 0;
        const cell = el('td');
        const button = el('button', count ? '' : 'zero', count ? (count > 999 ? '999+' : String(count)) : '·');
        button.type = 'button';
        button.disabled = count === 0;
        button.title = `${source.name} → ${target.name}: ${fmt(count)} liên kết`;
        button.setAttribute('aria-label', button.title);
        if (count) {
          const strength = Math.log1p(count) / Math.log1p(maxCell);
          button.style.backgroundColor = `rgba(87, 189, 180, ${(.13 + strength * .7).toFixed(2)})`;
          button.addEventListener('click', () => {
            chosenPair = [source.name, target.name];
            chosenModule = source.name;
            document.querySelectorAll('#moduleMatrix td button.active').forEach(node => node.classList.remove('active'));
            button.classList.add('active');
            document.querySelector('#clearModule').hidden = false;
            renderModuleDetail();
            renderResults();
          });
        }
        cell.append(button);
        row.append(cell);
      }
      body.append(row);
    }
    table.replaceChildren(head, body);
    renderModuleDetail();
  }

  function matchingScripts() {
    const query = document.querySelector('#scriptSearch').value.trim().toLocaleLowerCase('vi-VN');
    let pool = sortedScripts;
    if (chosenPair) {
      const [fromName, toName] = chosenPair;
      const paths = new Set();
      for (const edge of edges) {
        if (byPath.get(edge.from)?.module === fromName && byPath.get(edge.to)?.module === toName) {
          paths.add(edge.from);
          paths.add(edge.to);
        }
      }
      pool = pool.filter(script => paths.has(script.path));
    }
    return query ? pool.filter(script => script.path.toLocaleLowerCase('vi-VN').includes(query)) : pool;
  }

  function selectScript(path) {
    const script = byPath.get(path);
    if (!script) return;
    chosenScript = path;
    renderResults();
    renderRelationships();
  }

  function renderResults() {
    const host = document.querySelector('#scriptResults');
    const matches = matchingScripts();
    const visible = matches.slice(0, 80);
    document.querySelector('#finderMeta').textContent = chosenPair
      ? `${fmt(matches.length)} script trong quan hệ module đã chọn${matches.length > 80 ? ' · hiện 80, hãy tìm theo tên' : ''}`
      : `${fmt(matches.length)} script${matches.length > 80 ? ' · hiện 80 có nhiều liên kết nhất, hãy tìm theo tên' : ''}`;
    host.replaceChildren();
    if (!visible.length) { host.append(el('div', 'empty', 'Không tìm thấy script. Thử tên ngắn hơn hoặc bỏ lọc module.')); return; }
    for (const script of visible) {
      const button = el('button', chosenScript === script.path ? 'selected' : '');
      button.type = 'button';
      button.title = script.path;
      button.setAttribute('role', 'option');
      button.setAttribute('aria-selected', String(chosenScript === script.path));
      button.append(el('b', '', nameOf(script.path)), el('small', '', `${shortModule(script.module)} · ${fmt(degree(script.path))} liên kết`));
      button.addEventListener('click', () => selectScript(script.path));
      host.append(button);
    }
  }

  function openReference(edge) {
    const dialog = document.querySelector('#referenceDialog');
    const host = document.querySelector('#referenceBody');
    document.querySelector('#referenceTitle').textContent = 'Tham chiếu trong mã';
    host.replaceChildren();
    host.append(el('p', 'reference-route', `${edge.from}  →  ${edge.to}`));
    if (edge.details?.length) {
      const reasons = el('p', '', edge.details.join(' · '));
      host.append(reasons);
    }
    const refs = edge.references || [];
    host.append(el('h3', '', `Vị trí tham chiếu đã ghi nhận (${fmt(refs.length)})`));
    if (!refs.length) {
      host.append(el('p', 'empty', 'Phân tích tĩnh xác định được quan hệ giữa hai file nhưng chưa lưu được dòng tham chiếu chính xác.'));
    } else {
      const list = el('ol', 'reference-list');
      for (const ref of refs) {
        const item = el('li');
        item.append(el('div', 'location', `${edge.from}:${ref.sourceLine} → ${edge.to}:${ref.targetLine} · ${ref.symbol}`));
        item.append(el('pre', '', ref.snippet || ''));
        list.append(item);
      }
      host.append(list);
    }
    dialog.showModal();
  }

  function relationColumn(title, description, relatedEdges, neighborKey, arrow) {
    const column = el('div', 'relation-col');
    column.append(el('h4', '', `${title} (${fmt(relatedEdges.length)})`), el('p', '', description));
    const list = el('div', 'relation-list');
    if (!relatedEdges.length) list.append(el('div', 'relation-empty', 'Không tìm thấy liên kết theo chiều này.'));
    else {
      const sorted = [...relatedEdges].sort((a, b) => degree(b[neighborKey]) - degree(a[neighborKey]) || a[neighborKey].localeCompare(b[neighborKey]));
      for (const edge of sorted) {
        const neighbor = byPath.get(edge[neighborKey]);
        const row = el('div', 'relation-row');
        row.append(el('span', 'direction', arrow));
        const main = el('div', 'relation-main');
        const select = el('button', '', neighbor ? nameOf(neighbor.path) : edge[neighborKey]);
        select.type = 'button';
        select.title = edge[neighborKey];
        select.addEventListener('click', () => selectScript(edge[neighborKey]));
        main.append(select, el('small', '', neighbor ? shortModule(neighbor.module) : edge[neighborKey]));
        const reference = el('button', 'reference-button', 'Xem mã');
        reference.type = 'button';
        reference.setAttribute('aria-label', `Xem tham chiếu từ ${edge.from} đến ${edge.to}`);
        reference.addEventListener('click', () => openReference(edge));
        row.append(main, reference);
        list.append(row);
      }
    }
    column.append(list);
    return column;
  }

  function inheritanceLabel(relation) {
    if (relation.relation === 'implements') return 'triển khai interface';
    if (relation.relation === 'extends-interface') return 'kế thừa interface';
    if (relation.relation === 'extends') return 'kế thừa lớp';
    return 'kiểu ngoài project / chưa phân giải';
  }

  function openInheritance(relation) {
    const dialog = document.querySelector('#referenceDialog');
    const host = document.querySelector('#referenceBody');
    document.querySelector('#referenceTitle').textContent = 'Khai báo kế thừa';
    host.replaceChildren();
    host.append(el('p', 'reference-route', `${relation.sourceType} → ${relation.baseType}`));
    host.append(el('p', '', inheritanceLabel(relation)));
    host.append(el('div', 'location', `${relation.from}:${relation.line}${relation.to ? ` → ${relation.to}` : ''}`));
    host.append(el('pre', 'inheritance-code', relation.snippet || ''));
    if (!relation.to) host.append(el('p', 'empty', 'Không tìm thấy khai báo đích duy nhất trong project; có thể là kiểu thư viện hoặc tên chưa phân giải.'));
    dialog.showModal();
  }

  function inheritanceColumn(title, relations, parentSide) {
    const column = el('div', 'inheritance-col');
    column.append(el('h5', '', `${title} (${fmt(relations.length)})`));
    const list = el('div', 'inheritance-list');
    if (!relations.length) list.append(el('div', 'relation-empty', 'Không tìm thấy quan hệ ở chiều này.'));
    for (const relation of relations) {
      const row = el('div', 'inheritance-row');
      const targetPath = parentSide ? relation.to : relation.from;
      const primary = parentSide ? relation.baseType : relation.sourceType;
      const content = el('div', 'inheritance-main');
      if (targetPath) {
        const jump = el('button', 'inheritance-jump', primary);
        jump.type = 'button';
        jump.title = targetPath;
        jump.addEventListener('click', () => selectScript(targetPath));
        content.append(jump);
      } else content.append(el('strong', '', primary));
      content.append(el('small', '', `${inheritanceLabel(relation)} · ${parentSide ? relation.sourceType : relation.baseType}`));
      const evidence = el('button', 'reference-button', 'Khai báo');
      evidence.type = 'button';
      evidence.addEventListener('click', () => openInheritance(relation));
      row.append(content, evidence);
      list.append(row);
    }
    column.append(list);
    return column;
  }

  function renderRelationships() {
    const host = document.querySelector('#relationshipView');
    host.replaceChildren();
    const script = byPath.get(chosenScript);
    if (!script) { host.append(el('div', 'empty', 'Chọn một script bên trái để xem quan hệ của nó.')); return; }
    const head = el('div', 'selected-script');
    head.append(el('div', 'caption', 'Script đang xem'), el('h3', '', nameOf(script.path)), el('div', 'full-path', script.path));
    const metrics = el('div', 'metrics');
    for (const [label, value] of [['Dòng', script.lines], ['Code', script.code], ['Điểm rẽ nhánh ước tính', script.decisions],
                                  ['Kế thừa / triển khai', parents.get(script.path)?.length || 0],
                                  ['Được kế thừa / triển khai', children.get(script.path)?.length || 0]]) {
      const span = el('span');
      span.append(`${label}: `, el('b', '', fmt(value)));
      metrics.append(span);
    }
    head.append(metrics);
    const inheritanceSection = el('section', 'inheritance-section');
    inheritanceSection.append(el('h4', '', 'Kế thừa và interface'));
    inheritanceSection.append(el('p', '', 'Quan hệ từ khai báo lớp/interface; tách riêng khỏi các tham chiếu sử dụng thông thường.'));
    const inheritanceColumns = el('div', 'inheritance-columns');
    inheritanceColumns.append(
      inheritanceColumn('Lớp cha / interface', parents.get(script.path) || [], true),
      inheritanceColumn('Lớp con / nơi triển khai', children.get(script.path) || [], false)
    );
    inheritanceSection.append(inheritanceColumns);
    const columns = el('div', 'relation-columns');
    columns.append(
      relationColumn('Được dùng bởi', 'Các script này có tham chiếu tới script đang xem.', incoming.get(script.path) || [], 'from', '→'),
      relationColumn('Đang dùng', 'Script đang xem có tham chiếu tới các script này.', outgoing.get(script.path) || [], 'to', '→')
    );
    host.append(head, inheritanceSection, columns);
  }

  document.querySelector('#scriptSearch').addEventListener('input', renderResults);
  document.querySelector('#clearModule').addEventListener('click', () => {
    chosenPair = null;
    chosenModule = null;
    document.querySelector('#clearModule').hidden = true;
    document.querySelectorAll('#moduleMatrix td button.active').forEach(node => node.classList.remove('active'));
    renderModuleDetail();
    renderResults();
  });
  document.querySelector('#closeDialog').addEventListener('click', () => document.querySelector('#referenceDialog').close());
  document.querySelector('#referenceDialog').addEventListener('click', event => { if (event.target.id === 'referenceDialog') event.target.close(); });
  renderMatrix();
  renderResults();
  if (sortedScripts.length) selectScript(sortedScripts[0].path);
})();
