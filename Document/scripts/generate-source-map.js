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
const OUTPUT_FILE = path.resolve(__dirname, '../src/generated/source-code.json');

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

function readDirectory(dir, rootName) {
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

    // Calculate relative path from the source root, prefixed with rootName
    // e.g. /demo/000_Basic/README.md
    // We need to find which root this file belongs to, but here we are inside a recursive function
    // so we pass the root path down or calculate relative to the current root being processed.

    // Actually, let's make the key relative to the "virtual root" we are building.
    // If we are processing 'demo', and file is 'demo/README.md', key should be '/demo/README.md'.

    if (stat.isDirectory()) {
      Object.assign(result, readDirectory(fullPath, rootName));
    } else {
      try {
        // Read file content
        // Limit file size to avoid huge JSON
        if (stat.size > 1024 * 100) { // 100KB limit
          console.warn(`Skipping large file ${fullPath} (${stat.size} bytes)`);
          result[getVirtualPath(fullPath, rootName)] = { code: `// File too large to display (${Math.round(stat.size / 1024)}KB)` };
          return;
        }

        const content = fs.readFileSync(fullPath, 'utf-8');
        result[getVirtualPath(fullPath, rootName)] = { code: content };
      } catch (e) {
        console.warn(`Skipping file ${fullPath}: ${e.message}`);
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
  const allFiles = {};

  SOURCE_ROOTS.forEach(root => {
    if (!fs.existsSync(root.path)) {
      console.warn(`Source root not found: ${root.path}`);
      return;
    }
    console.log(`Processing ${root.name} from ${root.path}...`);
    const files = readDirectory(root.path, root.name);
    Object.assign(allFiles, files);
  });

  // Ensure output directory exists
  const outDir = path.dirname(OUTPUT_FILE);
  if (!fs.existsSync(outDir)) {
    fs.mkdirSync(outDir, { recursive: true });
  }

  fs.writeFileSync(OUTPUT_FILE, JSON.stringify(allFiles, null, 2));
  console.log(`Generated source map at ${OUTPUT_FILE} with ${Object.keys(allFiles).length} files.`);
}

generate();
