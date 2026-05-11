import { useState, useCallback, useRef, useEffect } from "react";
import { CellPosition, CellSelectionRange } from ".";
import useEvent from "react-use-event-hook";

export interface UseSelectionReturn {
  activeCell: CellPosition | null;
  selectedRange: CellSelectionRange | null;
  /** 範囲選択のアンカーセル */
  anchorCellRef: React.RefObject<CellPosition | null>;
  setActiveCell: (cell: CellPosition | null) => void;
  setSelectedRange: (range: CellSelectionRange | null) => void;
  handleMouseDown: (event: React.MouseEvent, rowIndex: number, colIndex: number) => void;
  handleMouseEnter: (rowIndex: number, colIndex: number) => void;
  selectRows: (startRowIndex: number, endRowIndex: number) => void;
}

export function useSelection(
  totalRows: number,
  totalColumns: number,
  showCheckBox: boolean,
  isGridActive: boolean,
  onActiveCellChanged: (cell: CellPosition | null) => void,
): UseSelectionReturn {

  //#region 状態

  // アクティブセル。
  // キー操作で選択範囲が伸縮するときの先端の位置。
  const [activeCell, setActiveCell_useState] = useState<CellPosition | null>(null);

  // 選択範囲。アクティブセルとアンカーセルから成る矩形範囲。
  // 選択されているセルが単一の場合は、アクティブセル、アンカーセルの範囲と一致する。
  const [selectedRange, setSelectedRange] = useState<CellSelectionRange | null>(null);

  // アンカーセル。
  // Shiftキーを押しながら範囲選択したときの、選択開始の最初のセルのこと。
  // 選択されているセルが単一の場合は、アクティブセルと同じ。
  const anchorCellRef = useRef<CellPosition | null>(null);

  // ドラッグ選択中かどうか
  const [isDragging, setIsDragging] = useState(false);

  //#endregion 状態

  // ----------------------------------------

  //#region マウスハンドラ

  // mouse down
  const handleMouseDown = useCallback((event: React.MouseEvent, rowIndex: number, colIndex: number) => {
    const currentCell = { rowIndex, colIndex };

    if (event.shiftKey && anchorCellRef.current) {
      // 範囲選択の選択範囲を拡張
      setActiveCell(currentCell);
      setSelectedRange({
        startRow: Math.min(anchorCellRef.current.rowIndex, currentCell.rowIndex),
        startCol: Math.min(anchorCellRef.current.colIndex, currentCell.colIndex),
        endRow: Math.max(anchorCellRef.current.rowIndex, currentCell.rowIndex),
        endCol: Math.max(anchorCellRef.current.colIndex, currentCell.colIndex)
      });
    } else {
      // 単一のセルを選択
      setActiveCell(currentCell);
      setSelectedRange({
        startRow: currentCell.rowIndex,
        startCol: currentCell.colIndex,
        endRow: currentCell.rowIndex,
        endCol: currentCell.colIndex
      });
    }

    // 単一のセルを選択した場合は、アンカーセルを更新
    if (!event.shiftKey) {
      anchorCellRef.current = currentCell;
    }

    // ドラッグ開始
    setIsDragging(true)
  }, [anchorCellRef]);

  // mouse enter
  const handleMouseEnter = useEvent((rowIndex: number, colIndex: number) => {
    if (isDragging && anchorCellRef.current) {
      // アクティブセルを更新
      setActiveCell_useState({ rowIndex, colIndex });
      onActiveCellChanged({ rowIndex, colIndex });

      // 選択範囲を更新
      setSelectedRange({
        startRow: Math.min(anchorCellRef.current.rowIndex, rowIndex),
        startCol: Math.min(anchorCellRef.current.colIndex, colIndex),
        endRow: Math.max(anchorCellRef.current.rowIndex, rowIndex),
        endCol: Math.max(anchorCellRef.current.colIndex, colIndex)
      });
    }
  })

  // mouse up
  useEffect(() => {
    const handleMouseUp = () => {
      setIsDragging(false)
    }
    document.addEventListener('mouseup', handleMouseUp)
    return () => {
      document.removeEventListener('mouseup', handleMouseUp)
    }
  }, [])

  //#endregion マウスハンドラ

  // ----------------------------------------

  //#region 選択範囲の強制補正

  // フォーカスイン時、アクティブセルが無ければ最初のセルを選択
  useEffect(() => {
    if (isGridActive && !activeCell && totalRows > 0 && totalColumns > 0) {
      const initialColIndex = showCheckBox ? 1 : 0;
      setActiveCell({ rowIndex: 0, colIndex: initialColIndex });
      setSelectedRange({
        startRow: 0,
        startCol: initialColIndex,
        endRow: 0,
        endCol: initialColIndex
      });
    }
  }, [isGridActive])

  // データ範囲の変更に応じて選択状態を調整
  useEffect(() => {
    const maxRowIndex = Math.max(0, totalRows - 1);
    const maxColIndex = Math.max(0, totalColumns - 1);

    // データが空になった場合は選択状態をクリア
    if (totalRows === 0 || totalColumns === 0) {
      setActiveCell_useState(null);
      setSelectedRange(null);
      anchorCellRef.current = null;
      return;
    }

    // アクティブセルが範囲外の場合は調整
    if (activeCell) {
      if (activeCell.rowIndex > maxRowIndex || activeCell.colIndex > maxColIndex) {
        const adjustedActiveCell = {
          rowIndex: Math.min(activeCell.rowIndex, maxRowIndex),
          colIndex: Math.min(activeCell.colIndex, maxColIndex)
        };
        setActiveCell_useState(adjustedActiveCell);
        onActiveCellChanged(adjustedActiveCell);
      }
    }

    // アンカーセルが範囲外の場合は調整
    if (anchorCellRef.current) {
      const currentAnchor = anchorCellRef.current;
      if (currentAnchor.rowIndex > maxRowIndex || currentAnchor.colIndex > maxColIndex) {
        anchorCellRef.current = {
          rowIndex: Math.min(currentAnchor.rowIndex, maxRowIndex),
          colIndex: Math.min(currentAnchor.colIndex, maxColIndex)
        };
      }
    }

    // 選択範囲が範囲外の場合は調整
    setSelectedRange(prevRange => {
      if (!prevRange) return null;

      const adjustedRange = {
        startRow: Math.min(prevRange.startRow, maxRowIndex),
        startCol: Math.min(prevRange.startCol, maxColIndex),
        endRow: Math.min(prevRange.endRow, maxRowIndex),
        endCol: Math.min(prevRange.endCol, maxColIndex)
      };

      if (adjustedRange.startRow !== prevRange.startRow ||
        adjustedRange.startCol !== prevRange.startCol ||
        adjustedRange.endRow !== prevRange.endRow ||
        adjustedRange.endCol !== prevRange.endCol) {
        return adjustedRange;
      }
      return prevRange;
    });

  }, [totalRows, totalColumns, onActiveCellChanged]);

  //#endregion 選択範囲の強制補正

  // ----------------------------------------

  //#region 外部公開

  // 行範囲選択
  const selectRows = useCallback((startRowIndex: number, endRowIndex: number) => {
    const newSelectedRows = new Set<number>();
    const min = Math.max(0, Math.min(startRowIndex, endRowIndex));
    const max = Math.min(totalRows - 1, Math.max(startRowIndex, endRowIndex));
    for (let i = min; i <= max; i++) {
      newSelectedRows.add(i);
    }
    setActiveCell({ rowIndex: min, colIndex: 0 });
    setSelectedRange({ startCol: 0, endCol: totalColumns - 1, startRow: min, endRow: max });
    anchorCellRef.current = { rowIndex: min, colIndex: 0 };
  }, [totalRows, totalColumns, anchorCellRef]);

  // キーボード操作のためのアクティブセル更新関数
  const setActiveCell = useCallback((cell: CellPosition | null) => {
    setActiveCell_useState(cell);
    onActiveCellChanged(cell);
  }, [setActiveCell_useState, onActiveCellChanged]);

  //#endregion 外部公開

  return {
    activeCell,
    selectedRange,
    anchorCellRef,
    setActiveCell,
    setSelectedRange,
    handleMouseDown,
    handleMouseEnter,
    selectRows
  };
}
