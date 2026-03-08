// available-files.js で指定されたファイルを
// リポジトリ上からコピーして static フォルダに格納する。
const fs = require("node:fs")
const path = require("node:path")

const WORKSPACE_ROOT = path.resolve(__dirname, "..", "..")
const DOCUMENT_ROOT = path.resolve(__dirname, "..")
const AVAILABLE_FILES_PATH = path.resolve(
	DOCUMENT_ROOT,
	"src/components/SourceCodeViewer/available-files.js",
)
const NIJO_XML_PATHS_PATH = path.resolve(
	DOCUMENT_ROOT,
	"src/components/NijoSchemaViewer/nijo-xml-paths.js",
)
const TARGET_ROOT = path.resolve(DOCUMENT_ROOT, "static/source-codes")

/**
 * `export default ...` を読み取って値を返す。
 */
function loadDefaultExport(filePath) {
	const source = fs.readFileSync(filePath, "utf8")
	const match = source.match(/export\s+default\s+([\s\S]+?)\s*;?\s*$/)

	if (!match) {
		throw new Error(`export default が見つかりません: ${filePath}`)
	}

	return Function(`"use strict"; return (${match[1]});`)()
}

function assertInside(basePath, targetPath, label) {
	const normalizedBase = `${path.resolve(basePath)}${path.sep}`
	const normalizedTarget = path.resolve(targetPath)

	if (!normalizedTarget.startsWith(normalizedBase)) {
		throw new Error(`${label} のパスが許可範囲外です: ${targetPath}`)
	}
}

function copyFile(workspaceRelativePath) {
	const sourcePath = path.resolve(WORKSPACE_ROOT, workspaceRelativePath)
	const destinationPath = path.resolve(TARGET_ROOT, workspaceRelativePath)

	assertInside(WORKSPACE_ROOT, sourcePath, "コピー元")
	assertInside(TARGET_ROOT, destinationPath, "コピー先")

	if (!fs.existsSync(sourcePath)) {
		throw new Error(`コピー元ファイルが存在しません: ${sourcePath}`)
	}

	fs.mkdirSync(path.dirname(destinationPath), { recursive: true })
	fs.copyFileSync(sourcePath, destinationPath)
	console.log(`copied: ${workspaceRelativePath}`)
}

function main() {
	const availableFiles = loadDefaultExport(AVAILABLE_FILES_PATH)
	const nijoXmlPaths = loadDefaultExport(NIJO_XML_PATHS_PATH)

	fs.rmSync(TARGET_ROOT, { recursive: true, force: true })

	let copiedCount = 0
	for (const [projectPath, files] of Object.entries(availableFiles)) {
		if (!Array.isArray(files)) {
			throw new Error(`ファイル一覧が配列ではありません: ${projectPath}`)
		}

		for (const filePath of files) {
			copyFile(path.join(projectPath, filePath))
			copiedCount += 1
		}
	}

	if (!Array.isArray(nijoXmlPaths)) {
		throw new Error(`nijo.xml 一覧が配列ではありません: ${NIJO_XML_PATHS_PATH}`)
	}

	for (const filePath of nijoXmlPaths) {
		copyFile(filePath)
		copiedCount += 1
	}

	console.log(`done: ${copiedCount} files copied to ${TARGET_ROOT}`)
}

main()
