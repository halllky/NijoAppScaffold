export default function () {
  return (
    // Root
    <div className="flex flex-col w-full items-stretch">

      {/* 動的に挿入するdiv */}
      <div className={`flex ${(false ? 'flex-col' : 'items-start')}`}>

        {/* 動的に挿入するdiv */}
        <div className="grid grid-cols-[120px_200px]">
          {/* FormItem 1個分 ここから */}
          <div className="bg-red-100">1</div>
          <div className="bg-red-100">1<br />1<br />1<br />1<br /></div>
          {/* FormItem 1個分 ここまで */}
          <div className="bg-sky-100">2</div>
          <div className="bg-sky-100 h-16">2</div>
          <div className="bg-orange-100">3</div>
          <div className="bg-orange-100">3</div>
        </div>
        {/* BreakPoint の位置、または要素数の半分がこの部分 */}
        <div className="flex-1 grid grid-cols-[120px_1fr]">
          {/* 入れ子セクション(not full width) */}
          <div className="grid col-span-2 grid-cols-[subgrid] border">
            <div className="col-span-2">
              ヘッダ
            </div>
            <div className="bg-yellow-100">4</div>
            <div className="bg-yellow-100">4</div>
            <div className="bg-blue-100">5</div>
            <div className="bg-blue-100">5</div>
            {/* 入れ子セクション(not full width) の中の full width は単に col-span-2 がつくだけ */}
            <div className="bg-purple-100 col-span-2">6(full width)</div>
            <div className="bg-purple-100 col-span-2">6値</div>
            <div className="bg-gray-100">7</div>
            <div className="bg-gray-100">7</div>
          </div>
          <div className="bg-pink-100">8</div>
          <div className="bg-pink-100">8</div>
          <div className="bg-gray-100">9</div>
          <div className="bg-gray-100">9</div>
        </div>
      </div>

      {/* full width */}
      <div className="bg-red-100">
        full width ラベル
      </div>
      <div className="bg-emerald-100">
        full width コンテンツ
      </div>
    </div>
  )
}