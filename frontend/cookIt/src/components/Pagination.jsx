import { Button } from "./ui/button";

const Pagination = ({ currentPage, totalPages, onPageChange }) => {
  if (totalPages <= 1) return null;

  return (
    <div className="flex justify-center my-8 items-center gap-2">
      <Button
        variant="link"
        onClick={() => onPageChange(Math.max(currentPage - 1, 1))}
        disabled={currentPage === 1}
        className="underline text-black disabled:text-gray-400 px-3 py-1 h-auto"
      >
        Назад
      </Button>
      {[...Array(totalPages)].map((_, i) => (
        <Button
          key={i + 1}
          variant={currentPage === i + 1 ? "violet" : "dashedHover"}
          onClick={() => onPageChange(i + 1)}
          className="rounded-2xl px-4 py-2 h-auto"
        >
          {i + 1}
        </Button>
      ))}
      <Button
        variant="link"
        onClick={() => onPageChange(Math.min(currentPage + 1, totalPages))}
        disabled={currentPage === totalPages}
        className="underline text-black disabled:text-gray-400 px-3 py-1 h-auto"
      >
        Вперед
      </Button>
    </div>
  );
};

export default Pagination;