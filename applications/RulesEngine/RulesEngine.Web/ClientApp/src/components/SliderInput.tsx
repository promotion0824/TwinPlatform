
interface SliderInputProps {
  absMin: number;
  absMax: number;
  resolution: number;
  min: number;
  max: number;
  leftDb: number;
  rightDb: number;
  setMin: any;
  setMax: any;
  setLeftDb: any;
  setRightDb: any;
}

const SliderInput: React.FC<SliderInputProps> = ({ absMin, absMax, resolution, min, max, leftDb, rightDb, setMin, setMax, setLeftDb, setRightDb }) => {

  const clampToRange = (n: number) => {
    if (n < absMin) {
      n = absMin;
    }
    if (n > absMax) {
      n = absMax;
    }
    return n;
  };

  return (
    <form>
      <div>
        <span>Min </span>
        <input type='number' placeholder='Add Min' value={Number.isNaN(min) ? '' : min} onChange={(e) => {
          let input = e.target.valueAsNumber;

          if (input >= leftDb) {
            input = leftDb - resolution;
          }
          input = clampToRange(input);

          setMin(Math.round(input / resolution) * resolution);
        }} />
      </div>
      <div>
        <span>Max </span>
        <input type='number' placeholder='Add Max' value={Number.isNaN(max) ? '' : max} onChange={(e) => {
          let input = e.target.valueAsNumber;

          if (input <= rightDb) {
            input = rightDb + resolution;
          }
          input = clampToRange(input);

          setMin(Math.round(input / resolution) * resolution);
        }} />
      </div>
      <div>
        <span>Left Deadband </span>
        <input type='number' placeholder='Add Left Deadband' value={Number.isNaN(leftDb) ? '' : leftDb} onChange={(e) => {
          let input = e.target.valueAsNumber;

          if (input >= rightDb) {
            input = rightDb - resolution;
          }
          if (input <= min) {
            input = min + resolution;
          }
          input = clampToRange(input);

          setMin(Math.round(input / resolution) * resolution);
        }} />
      </div>
      <div>
        <span>Right Deadband </span>
        <input type='number' placeholder='Add Right Deadband' value={Number.isNaN(rightDb) ? '' : rightDb} onChange={(e) => {
          let input = e.target.valueAsNumber;

          if (input >= max) {
            input = max - resolution;
          }
          if (input <= leftDb) {
            input = leftDb + resolution;
          }
          input = clampToRange(input);

          setMin(Math.round(input / resolution) * resolution);
        }} />
      </div>
    </form>
  );
}

export default SliderInput;
