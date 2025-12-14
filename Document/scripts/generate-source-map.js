// SourceCodeViewer によってドキュメント中に埋め込まれるソースコードのマップを生成するスクリプト

const fs = require('fs');
const path = require('path');

// Configuration
const SOURCE_ROOTS = [
  {
    name: 'demo',
    path: path.resolve(__dirname, '../../demo'),
  },
  // Add other roots if needed
];
const OUTPUT_FILE = path.resolve(__dirname, '../src/generated/file-tree.json');
const STATIC_ROOT = path.resolve(__dirname, '../static/source-code');

// Ignore patterns
const IGNORE_DIRS = [
  '.git',
  'node_modules',
  'bin',
  'obj',
  '.vs',
  '.vscode',
  'dist',
  'build',
  'coverage',
  'packages', // If monorepo packages are too large
];

const IGNORE_FILES = [
  '.DS_Store',
  'Thumbs.db',
  'package-lock.json',
  'yarn.lock',
];

const IGNORE_EXTENSIONS = [
  '.exe',
  '.dll',
  '.pdb',
  '.suo',
  '.user',
  '.cache',
  '.png',
  '.jpg',
  '.jpeg',
  '.gif',
  '.ico',
  '.pdf',
  '.zip',
  '.tar',
  '.gz',
];

function shouldIgnore(name, isDirectory) {
  if (isDirectory) {
    return IGNORE_DIRS.includes(name);
  }
  if (IGNORE_FILES.includes(name)) {
    return true;
  }
  const ext = path.extname(name).toLowerCase();
  if (IGNORE_EXTENSIONS.includes(ext)) {
    return true;
  }
  return false;
}

function processDirectory(dir, rootName) {
  const result = {};
  let items;
  try {
    items = fs.readdirSync(dir);
  } catch (e) {
    console.warn(`Failed to read directory ${dir}: ${e.message}`);
    return {};
  }

  items.forEach(item => {
    const fullPath = path.join(dir, item);
    let stat;
    try {
      stat = fs.statSync(fullPath);
    } catch (e) {
      console.warn(`Failed to stat ${fullPath}: ${e.message}`);
      return;
    }

    if (shouldIgnore(item, stat.isDirectory())) {
      return;
    }

    if (stat.isDirectory()) {
      Object.assign(result, processDirectory(fullPath, rootName));
    } else {
      try {
        const virtualPath = getVirtualPath(fullPath, rootName);

        // Copy file to static directory
        // virtualPath starts with /, e.g. /demo/folder/file.ts
        const relativePath = virtualPath.substring(1); // demo/folder/file.ts
        const destPath = path.join(STATIC_ROOT, relativePath);

        fs.mkdirSync(path.dirname(destPath), { recursive: true });
        fs.copyFileSync(fullPath, destPath);

        // Store metadata only
        result[virtualPath] = { size: stat.size };
      } catch (e) {
        console.warn(`Failed to process file ${fullPath}: ${e.message}`);
      }
    }
  });
  return result;
}

// Helper to construct the virtual path used in Sandpack
// We want the structure to look like:
// /demo/folder/file.ts
function getVirtualPath(fullPath, rootName) {
  const rootConfig = SOURCE_ROOTS.find(r => r.name === rootName);
  if (!rootConfig) return fullPath; // Should not happen

  const relative = path.relative(rootConfig.path, fullPath);
  // Ensure forward slashes for web
  const normalized = relative.split(path.sep).join('/');
  return `/${rootName}/${normalized}`;
}

function generate() {
  // Clean static directory
  if (fs.existsSync(STATIC_ROOT)) {
    console.log(`Cleaning ${STATIC_ROOT}...`);
    fs.rmSync(STATIC_ROOT, { recursive: true, force: true });
  }
  fs.mkdirSync(STATIC_ROOT, { recursive: true });

  const allFiles = {};

  SOURCE_ROOTS.forEach(root => {
    if (!fs.existsSync(root.path)) {
      console.warn(`Source root not found: ${root.path}`);
      return;
    }
    console.log(`Processing ${root.name} from ${root.path}...`);
    const files = processDirectory(root.path, root.name);
    Object.assign(allFiles, files);
  });

  // Ensure output directory exists
  const outDir = path.dirname(OUTPUT_FILE);
  if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir, { recursive: true });
  }

  fs.writeFileSync(OUTPUT_FILE, JSON.stringify(allFiles, null, 2));
  console.log(`Generated file tree at ${OUTPUT_FILE} with ${Object.keys(allFiles).length} files.`);
  console.log(`Source files copied to ${STATIC_ROOT}`);
}

generate();
