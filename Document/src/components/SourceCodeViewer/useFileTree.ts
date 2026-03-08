import React from "react"
import { SourceCodePathAndComment } from "./SourceCodeViewer"
import AVAILABLE_SOURCE_CODES from "./available-files"

export type FileTreeLeaf = {
  kind: "file"
  relativePath: string
  displayName: string
  indent: number
  comment: string
}

export type FileTreeFolder = {
  kind: "folder"
  relativePath: string
  displayName: string
  indent: number
}

export type FileTreeItem = FileTreeLeaf | FileTreeFolder

export function useFileTree(propsFiles: SourceCodePathAndComment<keyof typeof AVAILABLE_SOURCE_CODES>[]) {

  return React.useMemo((): FileTreeItem[] => {
    type TreeNode = {
      name: string
      path: string
      children: Map<string, TreeNode>
      file: { path: string, comment: string } | null
    }

    const root: TreeNode = {
      name: "",
      path: "",
      children: new Map(),
      file: null,
    }

    for (const file of propsFiles) {
      const parts = file.path.split("/").filter(Boolean)
      let current = root

      for (const part of parts) {
        let child = current.children.get(part)
        if (!child) {
          child = {
            name: part,
            path: current.path ? `${current.path}/${part}` : part,
            children: new Map(),
            file: null,
          }
          current.children.set(part, child)
        }
        current = child
      }

      current.file = {
        path: file.path,
        comment: file.comment,
      }
    }

    const flattened: FileTreeItem[] = []

    const visit = (node: TreeNode, depth: number): void => {
      if (node !== root && node.children.size > 0) {
        flattened.push({
          kind: "folder",
          relativePath: node.path,
          displayName: node.name,
          indent: Math.max(depth - 1, 0),
        })
      }

      if (node.file) {
        flattened.push({
          kind: "file",
          relativePath: node.file.path,
          displayName: node.name,
          indent: Math.max(depth - 1, 0),
          comment: node.file.comment,
        })
      }

      const children = [...node.children.values()]
      children.sort((a, b) => a.name.localeCompare(b.name))
      for (const child of children) {
        visit(child, depth + 1)
      }
    }

    visit(root, 0)
    return flattened
  }, [propsFiles])
}
