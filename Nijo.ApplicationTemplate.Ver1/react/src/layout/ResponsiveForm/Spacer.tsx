
/** グルーピングの境界線 */
export const Spacer = (props: {
  /** 線を引く */
  line?: boolean
}) => {
  return props.line ? (
    <hr className="col-span-full my-4 border-t border-gray-300" />
  ) : (
    <div className="col-span-full min-h-4 basis-4"></div>
  )
}