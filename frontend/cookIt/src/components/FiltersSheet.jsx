import React, { useState, useEffect } from "react";
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "./ui/sheet";
import { FaPepperHot } from "react-icons/fa";
import { GiWhisk } from "react-icons/gi";
import MultiSelect from "./ui/multi-select";

const RangeVisualRating = ({
  minValue,
  maxValue,
  onChangeMin,
  onChangeMax,
  max = 5,
  FilledIcon,
  OutlinedIcon,
  filledClass = "",
  outlinedClass = "text-gray-300",
  label
}) => {
  return (
    <div className="mb-4">
      <Label className="block mb-2 font-medium">{label}</Label>
      <div className="flex items-center mb-2">
        <span className="min-w-10 ml-1">От:</span>
        <div className="flex">
          {[...Array(max)].map((_, index) => {
            const value = index + 1;
            return (
              <button
                key={`min-${index}`}
                type="button"
                onClick={() => onChangeMin(value)}
                className="focus:outline-none mr-1"
              >
                {minValue >= value ? (
                  <FilledIcon className={`text-lg ${filledClass}`} />
                ) : (
                  <OutlinedIcon className={`text-lg ${outlinedClass}`} />
                )}
              </button>
            );
          })}
        </div>
      </div>
      <div className="flex items-center">
        <span className="min-w-10 ml-1">До:</span>
        <div className="flex">
          {[...Array(max)].map((_, index) => {
            const value = index + 1;
            return (
              <button
                key={`max-${index}`}
                type="button"
                onClick={() => onChangeMax(value)}
                className="focus:outline-none mr-1"
              >
                {maxValue >= value ? (
                  <FilledIcon className={`text-lg ${filledClass}`} />
                ) : (
                  <OutlinedIcon className={`text-lg ${outlinedClass}`} />
                )}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default function FiltersSheet({
  filters: externalFilters,
  onApplyFilters,
  dishTypes,
  equipments,
  ingredients,
}) {
  const [open, setOpen] = useState(false);
  const [localFilters, setLocalFilters] = useState(externalFilters);

  useEffect(() => {
    if (open) {
      setLocalFilters(externalFilters);
    }
  }, [open, externalFilters]);

  const handleFilterChange = (e) => {
    const { name, value, type } = e.target;
    if (type === 'multiselect') {
      setLocalFilters(prev => ({ ...prev, [name]: value }));
    } else {
      const val = value === '' ? null : value;
      setLocalFilters(prev => ({ ...prev, [name]: val }));
    }
  };

  const handleRangeChange = (field, value) => {
    setLocalFilters(prev => ({ ...prev, [field]: value }));
  };

  const handleApply = () => {
    onApplyFilters(localFilters);
    setOpen(false); 
  };

  const handleReset = () => {
    const defaultFilters = {
      searchText: '',
      minCookingTimeWithUser: null,
      maxCookingTimeWithUser: null,
      minCookingTimeWithoutUser: null,
      maxCookingTimeWithoutUser: null,
      minSpicinessLevel: null,
      maxSpicinessLevel: null,
      minDifficultyLevel: null,
      maxDifficultyLevel: null,
      dishTypeIds: [],
      equipmentIds: [],
      ingredientIds: []
    };
    setLocalFilters(defaultFilters);
  };

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="dashedHover">Фильтры</Button>
      </SheetTrigger>
      <SheetContent side="right" className="w-full sm:max-w-md overflow-y-auto bg-white">
        <SheetHeader>
          <SheetTitle>Фильтры</SheetTitle>
        </SheetHeader>
        <div className="mt-6 space-y-6">
          <div>
            <Label>Время с участием (мин)</Label>
            <div className="flex gap-2 mt-1">
              <Input
                type="number"
                name="minCookingTimeWithUser"
                value={localFilters.minCookingTimeWithUser || ''}
                onChange={handleFilterChange}
                placeholder="От"
              />
              <Input
                type="number"
                name="maxCookingTimeWithUser"
                value={localFilters.maxCookingTimeWithUser || ''}
                onChange={handleFilterChange}
                placeholder="До"
              />
            </div>
          </div>

          <div>
            <Label>Общее время (мин)</Label>
            <div className="flex gap-2 mt-1">
              <Input
                type="number"
                name="minCookingTimeWithoutUser"
                value={localFilters.minCookingTimeWithoutUser || ''}
                onChange={handleFilterChange}
                placeholder="От"
              />
              <Input
                type="number"
                name="maxCookingTimeWithoutUser"
                value={localFilters.maxCookingTimeWithoutUser || ''}
                onChange={handleFilterChange}
                placeholder="До"
              />
            </div>
          </div>

          <RangeVisualRating
            label="Острота"
            minValue={localFilters.minSpicinessLevel}
            maxValue={localFilters.maxSpicinessLevel}
            onChangeMin={(value) => handleRangeChange('minSpicinessLevel', value)}
            onChangeMax={(value) => handleRangeChange('maxSpicinessLevel', value)}
            FilledIcon={FaPepperHot}
            OutlinedIcon={FaPepperHot}
            filledClass="text-red-500"
          />
          <RangeVisualRating
            label="Сложность"
            minValue={localFilters.minDifficultyLevel}
            maxValue={localFilters.maxDifficultyLevel}
            onChangeMin={(value) => handleRangeChange('minDifficultyLevel', value)}
            onChangeMax={(value) => handleRangeChange('maxDifficultyLevel', value)}
            FilledIcon={GiWhisk}
            OutlinedIcon={GiWhisk}
            filledClass="text-green-500"
          />

          <div>
            <Label>Типы блюд</Label>
            <MultiSelect
              options={dishTypes}
              value={localFilters.dishTypeIds}
              onChange={handleFilterChange}
              name="dishTypeIds"
              placeholder="Выберите типы блюд"
            />
          </div>
          <div>
            <Label>Оборудование</Label>
            <MultiSelect
              options={equipments}
              value={localFilters.equipmentIds}
              onChange={handleFilterChange}
              name="equipmentIds"
              placeholder="Выберите оборудование"
            />
          </div>
          <div>
            <Label>Ингредиенты</Label>
            <MultiSelect
              options={ingredients}
              value={localFilters.ingredientIds}
              onChange={handleFilterChange}
              name="ingredientIds"
              placeholder="Выберите ингредиенты"
            />
          </div>

          <div className="flex gap-2 pt-4">
            <Button variant="violet" onClick={handleApply} className="flex-1">Применить</Button>
            <Button variant="dashedHover" onClick={handleReset} className="flex-1">Сбросить</Button>
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}