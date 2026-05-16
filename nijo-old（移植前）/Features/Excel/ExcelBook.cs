using Nijo.Util.CodeGenerating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nijo.Features.Excel {
    /// <summary>
    /// Excel入出力関連機能を提供するユーティリティクラス。
    /// </summary>
    internal class ExcelBook {

        internal const string BOOK_CLASS_NAME = "ExcelBook";
        internal const string SHEET_CLASS_NAME = "DataSheet";

        internal const string ADD_SHEET = "AddSheet";
        internal const string TO_BYTE_ARRAY = "ToByteArray";

        internal const string ADD_COLUMN = "AddColumn";
        internal const string RENDER_ROWS = "RenderRows";

        internal static SourceFile Render() => new SourceFile {
            FileName = "ExcelBook.cs",
            RenderContent = ctx => {

                return $$"""
                    using NPOI.SS.UserModel;
                    using NPOI.XSSF.UserModel;

                    namespace {{ctx.Config.RootNamespace}};

                    /// <summary>
                    /// Excel入出力関連機能を提供するユーティリティクラス。
                    /// </summary>
                    public partial class {{BOOK_CLASS_NAME}} {

                        /// <summary>
                        /// ブックを作成します。
                        /// </summary>
                        public ExcelBook() {
                            _book = new XSSFWorkbook();
                        }
                        private readonly XSSFWorkbook _book;

                        /// <summary>
                        /// このブックにシートを追加します。
                        /// </summary>
                        public virtual {{SHEET_CLASS_NAME}}<T> {{ADD_SHEET}}<T>(string sheetName) {
                            var npoiSheet = _book.GetSheet(sheetName) ?? _book.CreateSheet(sheetName);
                            var dataSheet = new DataSheet<T>(npoiSheet);
                            return dataSheet;
                        }

                        /// <summary>
                        /// このブックをバイト配列に変換します。
                        /// </summary>
                        public virtual byte[] {{TO_BYTE_ARRAY}}() {
                            using var stream = new NPOI.Util.ByteArrayOutputStream();
                            _book.Write(stream);
                            return stream.ToByteArray();
                        }
                    }

                    /// <summary>
                    /// <see cref="{{BOOK_CLASS_NAME}}"/> の内部で使用される、
                    /// Excelのシート1枚と対応するクラス。
                    /// </summary>
                    public partial class {{SHEET_CLASS_NAME}}<TItem> {

                        public DataSheet(ISheet sheet) {
                            _sheet = sheet;
                        }
                        private readonly ISheet _sheet;

                        /// <summary>
                        /// 1つのセルに入る最大長。これを超えると例外で落ちる
                        /// </summary>
                        private const int CELL_MAX_LENGTH = 32767;

                        /// <summary>
                        /// 最大列数
                        /// </summary>
                        private const int MAX_COLUMN_COUNT = 200;

                        // 列定義
                        private readonly List<ColumnDef> _columnDefs = new();
                        private class ColumnDef {
                            public required Func<TItem, object?> Getter { get; init; }
                            public required Type ColumnType { get; init; }
                            public required IEnumerable<string> Headers { get; init; }
                            public required int DecimalPlace { get; init; }
                        }

                        /// <summary>
                        /// <cref="{{RENDER_ROWS}}"/> したときにレンダリングされる列を定義する。
                        /// </summary>
                        public {{SHEET_CLASS_NAME}}<TItem> {{ADD_COLUMN}}<TProp>(Func<TItem, TProp> propertySelector, IEnumerable<string> columnHeaders, int decimalPlace = 0) {
                            _columnDefs.Add(new ColumnDef {
                                Getter = row => propertySelector(row),
                                ColumnType = typeof(TProp),
                                Headers = columnHeaders,
                                DecimalPlace = decimalPlace,
                            });
                            return this;
                        }

                        /// <summary>
                        /// <cref="{{ADD_COLUMN}}"/> で追加された列定義をもとに、引数のデータをExcelシートにレンダリングする。
                        /// </summary>
                        public void {{RENDER_ROWS}}(IEnumerable<TItem> items) {
                            // フォント
                            var datefont = _sheet.Workbook.CreateFont();
                            datefont.FontName = "BIZ UDゴシック";

                            // 日付時刻のセルが日付時刻でなく整数で表示されるのを防ぐ
                            var dateStyle = _sheet.Workbook.CreateCellStyle();
                            dateStyle.DataFormat = _sheet.Workbook.CreateDataFormat().GetFormat("yyyy/mm/dd hh:mm:ss");
                            dateStyle.SetFont(datefont);

                            var bodystartrownum = 1; //明細行描画開始位置
                            var beforeheadername = string.Empty;

                            //フォント設定
                            var fontstyle = _sheet.Workbook.CreateCellStyle();
                            var font = _sheet.Workbook.CreateFont();
                            font.FontName = "BIZ UDゴシック";
                            fontstyle.SetFont(font);

                            // 数値項目用のセルスタイル
                            var numCellStyle = _sheet.Workbook.CreateCellStyle();
                            numCellStyle.SetFont(font);
                            // 数値項目は右寄せで表示する
                            numCellStyle.Alignment = HorizontalAlignment.Right;
                            // 数値項目は3桁カンマ区切りで表示する
                            var dataFormat = _sheet.Workbook.CreateDataFormat();
                            numCellStyle.DataFormat = dataFormat.GetFormat("#,##0");

                            // 数値項目のフォーマット。「小数n桁以下」の設定ごとにインスタンスを用意する。
                            var dict = new Dictionary<int, ICellStyle>();
                            ICellStyle GetNumberCellStyle(int decimalPlace) {
                                if (decimalPlace == 0) {
                                    return numCellStyle; // 小数部なし
                            
                                } else if (dict.TryGetValue(decimalPlace, out var cellStyle)) {
                                    return cellStyle;
                            
                                } else {
                                    var newCellStyle = _sheet.Workbook.CreateCellStyle();
                                    newCellStyle.SetFont(font);
                                    // 数値項目小数点n桁まで表示用は右寄せで表示する
                                    newCellStyle.Alignment = HorizontalAlignment.Right;
                                    // 数値項目小数点n桁まで表示用は3桁カンマ区切りで表示する
                                    var decimalFormat = _sheet.Workbook.CreateDataFormat();
                                    // 小数点n桁まで表示する
                                    newCellStyle.DataFormat = decimalFormat.GetFormat($"#,##0.{new string('0', decimalPlace)}");
                            
                                    dict[decimalPlace] = newCellStyle;
                                    return newCellStyle;
                                }
                            }

                            // まず列ヘッダをレンダリングする
                            var headerRowUp = _sheet.CreateRow(0);   //ヘッダ行(上段)
                            var headerRowDown = _sheet.CreateRow(1); //ヘッダ行(下段)

                            for (int col = 0; col < _columnDefs.Count; col++) {
                                //ヘッダ行(上段)のセル
                                var cellUp = headerRowUp.CreateCell(col);
                                cellUp.CellStyle = fontstyle;

                                //ヘッダ行(下段)のセル
                                var cellDown = headerRowDown.CreateCell(col);
                                cellDown.CellStyle = fontstyle;

                                var headerArray = _columnDefs[col].Headers.ToArray(); //ヘッダ名称取得
                                var headercount = headerArray.Length;

                                if (headercount == 1) {
                                    //グループヘッダー無し
                                    cellUp.SetCellValue(headerArray[0]);

                                } else if (headercount == 2) {
                                    //グループヘッダー有り
                                    if (beforeheadername == string.Empty || beforeheadername != headerArray[0]) {
                                        beforeheadername = headerArray[0];
                                        cellUp.SetCellValue(headerArray[0]);

                                    } else if (beforeheadername == headerArray[0]) {
                                        //前回描画したグループヘッダ名称と同じなら描画しない
                                    }
                                    cellDown.SetCellValue(headerArray[1]);

                                    bodystartrownum = headercount;
                                }
                            }

                            //先頭行を固定する
                            _sheet.CreateFreezePane(0, bodystartrownum);

                            // オートフィルターをつける
                            _sheet.SetAutoFilter(new NPOI.SS.Util.CellRangeAddress(bodystartrownum - 1, bodystartrownum - 1, 0, _columnDefs.Count - 1));

                            // 次にデータをレンダリングする
                            // 大量データ出力時のパフォーマンス向上のため、一定件数ごとに分けて出力する
                            const int CHUNK_SIZE = 10000;
                            var chunkedData = items
                                .Select((item, index) => new { item, index })
                                .GroupBy(x => x.index / CHUNK_SIZE)
                                .Select(group => (group.Key, group.Select(g => g.item)));
                            foreach ((var key, var rows) in chunkedData) {
                                var startRowNumber = (key * CHUNK_SIZE) + bodystartrownum;
                                //レンダリング処理
                                var itemsArray = rows.ToArray();
                                for (int rowIndex = 0; rowIndex < itemsArray.Length; rowIndex++) { //検索結果の行数分ループ
                                    var item = itemsArray[rowIndex]; //1レコード取得
                                    var bodyRow = _sheet.CreateRow(rowIndex + startRowNumber); //詳細行の作成(ヘッダ行数分ずらす必要あり)

                                    for (int colIndex = 0; colIndex < _columnDefs.Count; colIndex++) { //列数分ループ
                                        var column = _columnDefs[colIndex];
                                        var value = column.Getter(item); //セル単位でデータ取得
                                        var cell = bodyRow.CreateCell(colIndex); //詳細行にセルを作成

                                        switch (value) {
                                            // 日付時刻型セル
                                            case DateTime dt:
                                                cell.SetCellValue(dt);
                                                cell.CellStyle = dateStyle;
                                                break;

                                            // 数値型セル
                                            case int:
                                            case uint:
                                            case long:
                                            case ulong:
                                            case sbyte:
                                            case byte:
                                            case short:
                                            case ushort:
                                            case float:
                                            case double:
                                            case decimal:
                                                cell.SetCellValue(Convert.ToDouble(value));
                                                // 数値項目用のCellStyleを設定
                                                cell.CellStyle = GetNumberCellStyle(column.DecimalPlace);
                    　　　　　　　　　　　　　　　　 break;

                                            // 真偽値
                                            case bool:
                                                cell.SetCellValue((bool)value);
                                                cell.CellStyle = fontstyle;
                                                break;

                                            // それ以外
                                            default:
                                                if (value != null) {
                                                    var str = value.ToString();
                                                    if (str != null && str.Length >= CELL_MAX_LENGTH) str = str.Substring(0, CELL_MAX_LENGTH - 16) + "(省略されました)";
                                                    cell.SetCellValue(str);
                                                    cell.CellStyle = fontstyle;
                                                }
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    """;
            },
        };
    }
}
