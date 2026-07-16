const VisualRating = ({ 
  value, 
  max, 
  FilledIcon, 
  OutlinedIcon,
  filledClass = "",
  outlinedClass = "text-gray-300",
  containerClass = ""
}) => {
  return (
    <div className={`flex items-center ${containerClass}`}>
      {[...Array(max)].map((_, index) => (
        <span key={index} className="mr-1">
          {index < value ? (
            <FilledIcon className={`text-xl ${filledClass}`} />
          ) : (
            <OutlinedIcon className={`text-xl ${outlinedClass}`} />
          )}
        </span>
      ))}
    </div>
  );
};

export default VisualRating;