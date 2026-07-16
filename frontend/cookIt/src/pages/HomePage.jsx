import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { RecipeWheelModal } from "../components/RecipeWheelModal";
import { RecipeDomeModal } from "../components/RecipeDomeModal"; 
import { ArrowRight, Dices, GalleryVerticalEnd } from "lucide-react";
import { useState } from "react";

export default function HomePage() {
  const [wheelOpen, setWheelOpen] = useState(false);
  const [domeOpen, setDomeOpen] = useState(false);

  return (
    <div className="h-full flex flex-col justify-center">
      <div className="-mx-8 mb-10">
        <img
          src="/cookit-line.svg"
          alt=""
          className="w-full h-[60px] object-cover"
        />
      </div>

      <div className="flex justify-center">
        <div className="flex items-center justify-center gap-16 max-w-6xl w-full">
          <div className="text-right max-w-lg">
            <img
              src="/cookit-heading.svg"
              alt="Heading"
              className="h-[230px] w-auto"
            />
            <p className="mt-6 text-lg text-gray-700 font-sans">
              Ищете вдохновение?<br />
              Посмотрите свежие рецепты или найдите <br /> что-то по душе в нашем каталоге.
            </p>
            <div className="flex justify-end gap-4 mt-8">
              <Button
                size="lg"
                variant="dashedHover"
                onClick={() => setWheelOpen(true)}
                className="gap-2"
              >
                <Dices className="h-4 w-4" />
                Рецепт от Фортуны
              </Button>
              <Button
                size="lg"
                variant="dashedHover"
                onClick={() => setDomeOpen(true)}
                className="gap-2"
              >
                <GalleryVerticalEnd className="h-4 w-4" />
                Галерея вкусов
              </Button>
            </div>
          </div>

          <img
            src="/main-image.svg"
            alt="Main visual"
            className="h-[400px] w-auto"
          />
        </div>
      </div>

      <div className="-mx-8 mt-10">
        <img
          src="/cookit-line.svg"
          alt=""
          className="w-full h-[60px] object-cover"
        />
      </div>

      <RecipeWheelModal isOpen={wheelOpen} onClose={() => setWheelOpen(false)} />
      <RecipeDomeModal isOpen={domeOpen} onClose={() => setDomeOpen(false)} />
    </div>
  );
}