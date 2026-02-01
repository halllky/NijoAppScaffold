import React from "react";
import useEvent from "react-use-event-hook";
import * as TanStack from "@tanstack/react-table";
import { EditableGrid2Props } from "./types-public";
import { CellSelectionRange } from "./useSelection";
import { ColumnMetadataInternal } from "./types-internal";
import { toTsvString, fromTsvString } from "../../util";
import { RowAccessor } from "./useRowAccessor";

interface UseCopyPasteParams<TRow> {
  table: TanStack.Table<TRow>;
  activeCell: { rowIndex: number; colIndex: number } | null;
  selectedRange: CellSelectionRange | null;
  /**
   * ペースト時に選択範囲を拡張したらここに新しい選択範囲が渡される
   */
  onRangeUpdated?: (range: CellSelectionRange) => void;
  isEditing: boolean;
  getRowObject: RowAccessor<TRow>;
  props: EditableGrid2Props<TRow>;
}

export const useCopyPaste = <TRow,>({
  table,
  activeCell,
  selectedRange,
  onRangeUpdated,
  isEditing,
  getRowObject,
  props,
}: UseCopyPasteParams<TRow>) => {

  const totalRowCount = Array.isArray(props.rows)
    ? props.rows.length
    : props.rows.array.length;

  const handleCopy: React.ClipboardEventHandler = useEvent(e => {
    if (isEditing || !selectedRange) return;

    if (totalRowCount === 0) return;

    e.preventDefault();
    e.stopPropagation();

    // 可視列（固定列含む）
    const columns = table.getVisibleLeafColumns();

    // 選択範囲内のセルの値を取得
    const dataArray: string[][] = [];
    for (let r = selectedRange.startRow; r <= selectedRange.endRow; r++) {
      const rowData: string[] = [];
      // 行データの存在チェック
      if (r >= totalRowCount) break;

      for (let c = selectedRange.startCol; c <= selectedRange.endCol; c++) {
        // 列定義の存在チェック
        if (c >= columns.length) break;

        const col = columns[c];
        const meta = col.columnDef.meta as ColumnMetadataInternal<TRow> | undefined;
        const colDef = meta?.original;

        let cellValue = '';
        if (colDef && colDef.getValueForEditor) {
          const row = getRowObject(r);
          if (row) {
            cellValue = colDef.getValueForEditor({ row, rowIndex: r });
          }
        }
        rowData.push(cellValue);
      }
      dataArray.push(rowData);
    }

    const tsvData = toTsvString(dataArray);
    if (e.clipboardData) {
      e.clipboardData.setData('text/plain', tsvData);
    }
  });

  const handlePaste: React.ClipboardEventHandler = useEvent(e => {
    if (isEditing || !activeCell) return;

    // ペースト開始セルの読み取り専用チェック
    // （ループ内でもチェックするが、開始地点がダメなら全体をキャンセルするかどうか。
    //   EditableGrid (v1) の挙動に合わせて、ここでは開始セルのチェックは行わない。
    //   各セルごとにチェックして、書き込み可能な場所だけ書き込む。）

    e.preventDefault();
    e.stopPropagation();

    try {
      const clipboardText = e.clipboardData?.getData('text/plain') || '';
      const pastedData = fromTsvString(clipboardText);

      setStringValuesToSelectedRange(pastedData);
    } catch (err) {
      console.error('クリップボードからのペーストに失敗しました:', err);
    }
  });

  const setStringValuesToSelectedRange = useEvent((values: string[][]) => {
    if (!activeCell) return;

    // ペーストデータのうち長さ0の配列部分は長さ1の配列と読み替える
    if (values.length === 0) {
      values = [['']];
    } else {
      for (let i = 0; i < values.length; i++) {
        if (values[i].length === 0) {
          values[i] = [''];
        }
      }
    }

    const startRow = selectedRange ? selectedRange.startRow : activeCell.rowIndex;
    const startCol = selectedRange ? selectedRange.startCol : activeCell.colIndex;

    // ペースト範囲の拡張が発生した場合はペースト後にセルの範囲選択を実行する
    let extendedRange = false;

    // ペースト先の下辺の行インデックス
    let endRow: number;
    const isOneCellSelected = !selectedRange || (selectedRange.startRow === selectedRange.endRow && selectedRange.startCol === selectedRange.endCol);

    if (isOneCellSelected) {
      endRow = startRow + values.length - 1;
      extendedRange = true;
    } else if (selectedRange) {
      endRow = selectedRange.endRow;
    } else {
      // Fallback (should be covered by isOneCellSelected if logic is correct)
      endRow = activeCell.rowIndex;
    }

    // ペースト先の右辺の列インデックス
    let endCol: number;
    if (isOneCellSelected) {
      endCol = startCol + values[0].length - 1;
      extendedRange = true;
    } else if (selectedRange) {
      endCol = selectedRange.endCol;
    } else {
      endCol = activeCell.colIndex;
    }

    const rowCount = endRow - startRow + 1;
    const colCount = endCol - startCol + 1;
    const columns = table.getVisibleLeafColumns();

    for (let r = 0; r < rowCount; r++) {
      const targetRowIndex = startRow + r;
      if (targetRowIndex >= totalRowCount) break;

      const pasteRowIdx = r % values.length;
      const rowInputData = values[pasteRowIdx];
      if (!rowInputData.length) continue;


      for (let c = 0; c < colCount; c++) {
        const targetColIndex = startCol + c;
        if (targetColIndex >= columns.length) break;

        const pasteColIdx = c % rowInputData.length;
        const col = columns[targetColIndex];
        const meta = col.columnDef.meta as ColumnMetadataInternal<TRow> | undefined;
        const colDef = meta?.original;

        // 必要な関数が定義されていないならスキップ
        if (!colDef || !colDef.setValueFromEditor) continue;

        const row = getRowObject(targetRowIndex);

        // 読み取り専用ならスキップ
        if (checkIfCellReadOnlyForPaste(props.isReadOnly, row, targetRowIndex, meta)) continue;

        const pasteValue = rowInputData[pasteColIdx];

        colDef.setValueFromEditor({
          row,
          rowIndex: targetRowIndex,
          value: pasteValue
        });
      }
    }

    if (extendedRange && onRangeUpdated) {
      onRangeUpdated({
        startRow,
        startCol,
        endRow,
        endCol
      });
    }
  });

  return { handleCopy, handlePaste };
};

function checkIfCellReadOnlyForPaste<TRow>(
  gridIsReadOnly: boolean | ((row: TRow, rowIndex: number) => boolean) | undefined,
  row: TRow,
  rowIndex: number,
  meta: ColumnMetadataInternal<TRow> | undefined
): boolean {
  if (gridIsReadOnly === true) return true;
  if (typeof gridIsReadOnly === 'function' && gridIsReadOnly(row, rowIndex)) return true;
  if (meta?.isReadOnly === true) return true;
  if (typeof meta?.isReadOnly === 'function' && meta.isReadOnly(row, rowIndex)) return true;
  return false;
}
