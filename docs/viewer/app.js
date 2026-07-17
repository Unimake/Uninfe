(function () {
  "use strict";

  const state = {
    manifest: null,
    searchIndex: null,
    currentPath: "",
    currentMarkdown: "",
    preferredInitialPath: "introducao/o-que-e-uninfe.md"
  };

  const elements = {
    navList: document.getElementById("nav-list"),
    content: document.getElementById("document-content"),
    status: document.getElementById("status-message"),
    currentPath: document.getElementById("current-path"),
    searchInput: document.getElementById("search-input"),
    searchResults: document.getElementById("search-results"),
    menuToggle: document.getElementById("menu-toggle"),
    themeToggle: document.getElementById("theme-toggle")
  };

  marked.setOptions({
    gfm: true,
    breaks: false,
    mangle: false,
    headerIds: true
  });

  mermaid.initialize({
    startOnLoad: false,
    securityLevel: "strict",
    theme: document.documentElement.dataset.theme === "dark" ? "dark" : "default"
  });

  function normalizeText(value) {
    return String(value || "")
      .toLowerCase()
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "");
  }

  function setStatus(message, isError) {
    elements.status.hidden = !message;
    elements.status.textContent = message || "";
    elements.status.style.color = isError ? "var(--danger)" : "var(--muted)";
  }

  function safeDocPath(pathValue) {
    const decoded = decodeURIComponent(String(pathValue || "")).replace(/^\/+/, "");
    if (!decoded || decoded.includes("\\") || decoded.includes("..") || !decoded.toLowerCase().endsWith(".md")) {
      return "";
    }

    return decoded;
  }

  function docUrl(pathValue) {
    return "../" + pathValue.split("/").map(encodeURIComponent).join("/");
  }

  function docHref(pathValue) {
    const url = new URL(window.location.href);
    url.searchParams.set("doc", pathValue);
    url.hash = "";
    return url.pathname + url.search + url.hash;
  }

  function getUrlPath() {
    const params = new URLSearchParams(window.location.search);
    return safeDocPath(params.get("doc")) || safeDocPath(window.location.hash.replace(/^#\/?/, ""));
  }

  function setDocUrl(pathValue, replace) {
    const href = docHref(pathValue);
    if (replace) {
      window.history.replaceState({ path: pathValue }, "", href);
      return;
    }

    window.history.pushState({ path: pathValue }, "", href);
  }

  function navigateTo(pathValue, replace) {
    const targetPath = safeDocPath(pathValue);
    if (!targetPath) {
      return;
    }

    if (targetPath !== state.currentPath || replace) {
      setDocUrl(targetPath, replace);
      loadDocument(targetPath);
    }

    closeMobileMenu();
  }

  function titleFor(pathValue) {
    const doc = state.manifest.documents.find((item) => item.path === pathValue);
    return doc ? doc.title : pathValue;
  }

  function groupDocuments(documents) {
    return documents.reduce((groups, doc) => {
      const category = doc.category || "Raiz";
      if (!groups[category]) {
        groups[category] = [];
      }
      groups[category].push(doc);
      return groups;
    }, {});
  }

  function categoryRank(category) {
    const value = normalizeText(category || "Raiz");
    const rank = {
      introducao: 10,
      instalacao: 20,
      configuracao: 30,
      referencias: 40,
      servicos: 50,
      raiz: 900
    };

    if (Object.prototype.hasOwnProperty.call(rank, value)) {
      return rank[value];
    }

    if (value.startsWith("servicos/")) {
      return 60;
    }

    return 800;
  }

  function compareCategories(a, b) {
    return categoryRank(a) - categoryRank(b) || a.localeCompare(b, "pt-BR");
  }

  function renderNavigation() {
    const groups = groupDocuments(state.manifest.documents);
    elements.navList.innerHTML = "";

    Object.keys(groups).sort(compareCategories).forEach((category) => {
      const section = document.createElement("section");
      section.className = "category";

      const title = document.createElement("h2");
      title.className = "category-title";
      title.textContent = category;
      section.appendChild(title);

      groups[category].forEach((doc) => {
        const link = document.createElement("a");
        const label = document.createElement("span");
        link.className = "nav-link";
        link.href = docHref(doc.path);
        link.dataset.path = doc.path;
        link.title = doc.title;
        label.textContent = doc.title;
        link.appendChild(label);
        link.addEventListener("click", (event) => {
          event.preventDefault();
          navigateTo(doc.path, false);
        });
        section.appendChild(link);
      });

      elements.navList.appendChild(section);
    });

    updateActiveNav();
  }

  function updateActiveNav() {
    let activeLink = null;
    document.querySelectorAll(".nav-link").forEach((link) => {
      const isActive = link.dataset.path === state.currentPath;
      link.classList.toggle("active", isActive);
      if (isActive) {
        activeLink = link;
      }
    });

    if (activeLink) {
      activeLink.scrollIntoView({ block: "nearest" });
    }
  }

  function escapeHtml(value) {
    return String(value || "")
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#39;");
  }

  function rewriteMarkdownLinks(container, basePath) {
    const baseParts = basePath.split("/");
    baseParts.pop();

    container.querySelectorAll("a[href]").forEach((link) => {
      const href = link.getAttribute("href");
      if (!href || href.startsWith("#") || /^[a-z][a-z0-9+.-]*:/i.test(href)) {
        if (/^https?:\/\//i.test(href)) {
          link.target = "_blank";
          link.rel = "noopener noreferrer";
        }
        return;
      }

      const [filePart] = href.split("#");
      if (!filePart.toLowerCase().endsWith(".md")) {
        return;
      }

      const normalized = normalizeRelativePath(baseParts.concat(filePart.split("/")));
      if (!normalized) {
        return;
      }

      link.href = docHref(normalized);
      link.addEventListener("click", (event) => {
        event.preventDefault();
        navigateTo(normalized, false);
      });
    });
  }

  function normalizeRelativePath(parts) {
    const stack = [];
    for (const part of parts) {
      if (!part || part === ".") {
        continue;
      }
      if (part === "..") {
        stack.pop();
        continue;
      }
      stack.push(part);
    }

    return safeDocPath(stack.join("/"));
  }

  function prepareMermaid(container) {
    const blocks = container.querySelectorAll("pre code.language-mermaid");
    blocks.forEach((code, index) => {
      const wrapper = document.createElement("div");
      wrapper.className = "mermaid-wrapper";

      const mermaidBlock = document.createElement("div");
      mermaidBlock.className = "mermaid";
      mermaidBlock.id = "mermaid-" + Date.now() + "-" + index;
      mermaidBlock.textContent = code.textContent;

      const original = document.createElement("pre");
      const originalCode = document.createElement("code");
      originalCode.textContent = code.textContent;
      original.appendChild(originalCode);
      original.hidden = true;

      wrapper.appendChild(mermaidBlock);
      wrapper.appendChild(original);
      code.closest("pre").replaceWith(wrapper);
    });
  }

  async function renderMermaid() {
    const diagrams = document.querySelectorAll(".mermaid-wrapper .mermaid");
    for (const diagram of diagrams) {
      const source = diagram.textContent;
      try {
        const result = await mermaid.render(diagram.id + "-svg", source);
        diagram.innerHTML = result.svg;
      } catch (error) {
        const wrapper = diagram.closest(".mermaid-wrapper");
        wrapper.classList.add("mermaid-error");
        const original = wrapper.querySelector("pre");
        original.hidden = false;
        diagram.innerHTML = `<strong>Nao foi possivel renderizar este fluxograma Mermaid.</strong><p>Confira a sintaxe do bloco abaixo.</p>`;
      }
    }
  }

  async function loadDocument(pathValue) {
    const targetPath = safeDocPath(pathValue) || defaultDocumentPath();
    if (!targetPath) {
      setStatus("Nenhum documento Markdown foi encontrado no manifesto.", true);
      return;
    }

    state.currentPath = targetPath;
    updateActiveNav();
    elements.currentPath.textContent = titleFor(targetPath);
    setStatus("Carregando " + targetPath + "...", false);

    try {
      const response = await fetch(docUrl(targetPath), { cache: "no-cache" });
      if (!response.ok) {
        throw new Error("HTTP " + response.status);
      }

      state.currentMarkdown = await response.text();
      const rawHtml = marked.parse(state.currentMarkdown);
      const cleanHtml = DOMPurify.sanitize(rawHtml, {
        USE_PROFILES: { html: true }
      });

      elements.content.innerHTML = cleanHtml;
      rewriteMarkdownLinks(elements.content, targetPath);
      prepareMermaid(elements.content);
      await renderMermaid();
      setStatus("", false);
      elements.content.focus({ preventScroll: true });
      document.title = titleFor(targetPath) + " - Documentacao UniNFe";
    } catch (error) {
      elements.content.innerHTML = "";
      setStatus("Nao foi possivel carregar o documento. Verifique se o arquivo existe e se o site esta sendo servido por HTTP/HTTPS.", true);
    }
  }

  function defaultDocumentPath() {
    const preferredDoc = state.manifest.documents.find((doc) => doc.path.toLowerCase() === state.preferredInitialPath);
    if (preferredDoc) {
      return preferredDoc.path;
    }

    const indexDoc = state.manifest.documents.find((doc) => doc.path.toLowerCase() === "index.md");
    return indexDoc ? indexDoc.path : (state.manifest.documents[0] && state.manifest.documents[0].path);
  }

  function isMobileLayout() {
    return window.matchMedia("(max-width: 860px)").matches;
  }

  function updateMenuButtonLabel() {
    const isMobile = isMobileLayout();
    const isOpen = document.body.classList.contains("menu-open");
    const isCollapsed = document.body.classList.contains("menu-collapsed");
    const label = isMobile
      ? (isOpen ? "Fechar menu" : "Abrir menu")
      : (isCollapsed ? "Abrir menu" : "Recolher menu");

    elements.menuToggle.setAttribute("aria-label", label);
    elements.menuToggle.setAttribute("title", label);
  }

  function syncMenuForViewport() {
    if (isMobileLayout()) {
      document.body.classList.remove("menu-open");
    }
    updateMenuButtonLabel();
  }

  function closeMobileMenu() {
    document.body.classList.remove("menu-open");
    updateMenuButtonLabel();
  }

  function searchDocuments(term) {
    const normalizedTerm = normalizeText(term);
    if (!normalizedTerm) {
      elements.searchResults.hidden = true;
      elements.searchResults.innerHTML = "";
      return;
    }

    const results = state.searchIndex.documents
      .map((doc) => {
        const haystack = normalizeText([
          doc.title,
          doc.path,
          doc.category,
          (doc.headings || []).map((heading) => heading.text).join(" "),
          doc.text
        ].join(" "));

        return haystack.includes(normalizedTerm) ? doc : null;
      })
      .filter(Boolean)
      .slice(0, 25);

    renderSearchResults(results, term);
  }

  function renderSearchResults(results, term) {
    elements.searchResults.hidden = false;
    if (!results.length) {
      elements.searchResults.innerHTML = `<p class="result-count">Nenhum resultado para "${escapeHtml(term)}".</p>`;
      return;
    }

    elements.searchResults.innerHTML = `<p class="result-count">${results.length} resultado(s)</p>`;
    results.forEach((doc) => {
      const link = document.createElement("a");
      link.className = "result-link";
      link.href = docHref(doc.path);
      link.innerHTML = `<strong>${escapeHtml(doc.title)}</strong><span>${escapeHtml(doc.category)}</span><small>${escapeHtml(snippet(doc.text, term))}</small>`;
      link.addEventListener("click", (event) => {
        event.preventDefault();
        elements.searchResults.hidden = true;
        navigateTo(doc.path, false);
      });
      elements.searchResults.appendChild(link);
    });
  }

  function snippet(text, term) {
    const clean = String(text || "").replace(/\s+/g, " ").trim();
    const normalizedClean = normalizeText(clean);
    const index = normalizedClean.indexOf(normalizeText(term));
    const start = Math.max(0, index > -1 ? index - 70 : 0);
    return clean.slice(start, start + 170) + (clean.length > start + 170 ? "..." : "");
  }

  async function loadJson(url, friendlyName) {
    const response = await fetch(url, { cache: "no-cache" });
    if (!response.ok) {
      throw new Error(`Nao foi possivel carregar ${friendlyName}.`);
    }
    return response.json();
  }

  async function boot() {
    try {
      state.manifest = await loadJson("docs-manifest.json", "o manifesto");
      renderNavigation();
    } catch (error) {
      setStatus("Nao foi possivel carregar docs-manifest.json. Execute node viewer/build-docs-index.js e publique o arquivo gerado.", true);
      return;
    }

    try {
      state.searchIndex = await loadJson("search-index.json", "o indice de busca");
    } catch (error) {
      state.searchIndex = { documents: [] };
      setStatus("O indice de busca nao foi carregado. A navegacao continua disponivel, mas a pesquisa pode nao funcionar.", true);
    }

    const initialPath = getUrlPath() || defaultDocumentPath();
    if (initialPath) {
      setDocUrl(initialPath, true);
    }

    await loadDocument(initialPath);
  }

  window.addEventListener("popstate", () => {
    loadDocument(getUrlPath());
    closeMobileMenu();
  });

  elements.searchInput.addEventListener("input", (event) => {
    searchDocuments(event.target.value);
  });

  elements.menuToggle.addEventListener("click", () => {
    if (isMobileLayout()) {
      document.body.classList.toggle("menu-open");
    } else {
      document.body.classList.toggle("menu-collapsed");
    }
    updateMenuButtonLabel();
  });

  window.addEventListener("resize", syncMenuForViewport);

  elements.themeToggle.addEventListener("click", () => {
    const nextTheme = document.documentElement.dataset.theme === "dark" ? "" : "dark";
    document.documentElement.dataset.theme = nextTheme;
    localStorage.setItem("uninfe-docs-theme", nextTheme);
    mermaid.initialize({
      startOnLoad: false,
      securityLevel: "strict",
      theme: nextTheme === "dark" ? "dark" : "default"
    });
    if (state.currentPath) {
      loadDocument(state.currentPath);
    }
  });

  const storedTheme = localStorage.getItem("uninfe-docs-theme");
  if (storedTheme) {
    document.documentElement.dataset.theme = storedTheme;
  }

  syncMenuForViewport();
  boot();
})();
