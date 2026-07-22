const fs = require("fs");
const path = require("path");

const docsRoot = path.resolve(__dirname, "..");
const viewerRoot = __dirname;
const ignoredDirs = new Set(["viewer", "node_modules", ".git"]);
const ignoredFileSuffixes = [".tmp", ".temp", ".bak", ".swp"];

function normalizePath(value) {
  return value.split(path.sep).join("/");
}

function isHidden(name) {
  return name.startsWith(".");
}

function shouldIgnoreFile(name) {
  const lower = name.toLowerCase();
  return isHidden(name)
    || lower === "readme.md"
    || lower === "_catalogo-documentacao.md"
    || ignoredFileSuffixes.some((suffix) => lower.endsWith(suffix));
}

function walk(directory, files) {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    if (isHidden(entry.name)) {
      continue;
    }

    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      if (!ignoredDirs.has(entry.name)) {
        walk(fullPath, files);
      }
      continue;
    }

    if (entry.isFile() && !shouldIgnoreFile(entry.name) && entry.name.toLowerCase().endsWith(".md")) {
      files.push(fullPath);
    }
  }
}

function titleFromFileName(filePath) {
  const base = path.basename(filePath, ".md");
  if (base.toLowerCase() === "readme") {
    return path.basename(path.dirname(filePath));
  }

  return base
    .replace(/[-_]+/g, " ")
    .replace(/\b\w/g, (letter) => letter.toUpperCase());
}

function extractTitle(markdown, filePath) {
  const match = markdown.match(/^#\s+(.+)$/m);
  return match ? match[1].trim() : titleFromFileName(filePath);
}

function extractHeadings(markdown) {
  const headings = [];
  const headingRegex = /^(#{1,6})\s+(.+)$/gm;
  let match;

  while ((match = headingRegex.exec(markdown)) !== null) {
    headings.push({
      level: match[1].length,
      text: match[2].trim()
    });
  }

  return headings;
}

function stripMarkdown(markdown) {
  return markdown
    .replace(/```[\s\S]*?```/g, " ")
    .replace(/`([^`]+)`/g, "$1")
    .replace(/!\[[^\]]*\]\([^)]+\)/g, " ")
    .replace(/\[([^\]]+)\]\([^)]+\)/g, "$1")
    .replace(/[#>*_\-|]+/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

function categoryFromRelativePath(relativePath) {
  const parts = relativePath.split("/");
  if (parts.length === 1) {
    return "Raiz";
  }

  return parts.slice(0, -1).join("/");
}

function categoryRank(category) {
  const value = String(category || "Raiz").toLowerCase();
  const rank = {
    "introducao": 10,
    "instalacao": 20,
    "configuracao": 30,
    "contingencia": 40,
    "erros-e-solucoes": 50,
    "referencias": 60,
    "servicos": 70,
    "raiz": 900
  };

  if (Object.prototype.hasOwnProperty.call(rank, value)) {
    return rank[value];
  }

  if (value.startsWith("servicos/")) {
    return 70;
  }

  return 800;
}

function compareCategories(a, b) {
  return categoryRank(a) - categoryRank(b) || String(a).localeCompare(String(b), "pt-BR");
}

function sortDocs(a, b) {
  return compareCategories(a.category, b.category) || a.title.localeCompare(b.title, "pt-BR");
}

const files = [];
walk(docsRoot, files);

const documents = files.map((filePath) => {
  const markdown = fs.readFileSync(filePath, "utf8");
  const relativePath = normalizePath(path.relative(docsRoot, filePath));
  const category = categoryFromRelativePath(relativePath);
  const headings = extractHeadings(markdown);
  const title = extractTitle(markdown, filePath);

  return {
    id: relativePath.replace(/[^a-zA-Z0-9]+/g, "-").replace(/^-|-$/g, "").toLowerCase(),
    title,
    path: relativePath,
    category,
    headings,
    text: stripMarkdown(markdown)
  };
}).sort(sortDocs);

const manifest = {
  generatedAt: new Date().toISOString(),
  documents: documents.map(({ id, title, path: docPath, category }) => ({
    id,
    title,
    path: docPath,
    category
  }))
};

const searchIndex = {
  generatedAt: manifest.generatedAt,
  documents
};

fs.writeFileSync(path.join(viewerRoot, "docs-manifest.json"), JSON.stringify(manifest, null, 2) + "\n", "utf8");
fs.writeFileSync(path.join(viewerRoot, "search-index.json"), JSON.stringify(searchIndex, null, 2) + "\n", "utf8");

console.log(`${documents.length} documentos indexados.`);
