const VisualRatingSelect = ({ 
  value, 
  onChange, 
  max = 5, 
  FilledIcon, 
  OutlinedIcon,
  filledClass = "",
  outlinedClass = "text-gray-300",
  containerClass = ""
}) => {
  return (
    <div className={`flex ${containerClass}`}>
      {[...Array(max)].map((_, index) => {
        const ratingValue = index + 1;
        return (
          <button
            key={index}
            type="button"
            onClick={() => onChange(ratingValue)}
            className="focus:outline-none mr-1"
            aria-label={`Установить ${ratingValue}`}
          >
            {value >= ratingValue ? (
              <FilledIcon className={`text-xl ${filledClass}`} />
            ) : (
              <OutlinedIcon className={`text-xl ${outlinedClass}`} />
            )}
          </button>
        );
      })}
    </div>
  );
};

export default VisualRatingSelect;