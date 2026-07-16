import { useState, useEffect, useMemo } from 'react';
import { Button } from "./ui/button";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { Card, CardContent } from "./ui/card";
import { ScrollArea } from "./ui/scroll-area";

const IngredientPicker = ({ 
  ingredients, 
  units, 
  value = [], 
  onChange, 
  name 
}) => {
  const [selectedIngredients, setSelectedIngredients] = useState(value);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedIngredientForAdd, setSelectedIngredientForAdd] = useState(null);
  const [unitId, setUnitId] = useState('');
  const [quantity, setQuantity] = useState('');

  useEffect(() => {
    setSelectedIngredients(value);
  }, [value]);

  const checkIfQuantityRequired = (unitIdToCheck) => {
    if (!unitIdToCheck) return false;
    const unit = units.find(u => u.id === parseInt(unitIdToCheck));
    if (!unit) return false;
    const unitName = unit.name.toLowerCase();
    return !unitName.includes('вкусу');
  };

  const isQuantityRequired = useMemo(() => {
    return checkIfQuantityRequired(unitId);
  }, [unitId, units]);

  const resetAddForm = () => {
    setSelectedIngredientForAdd(null);
    setUnitId('');
    setQuantity('');
    setSearchTerm('');
  };

  const filteredIngredients = useMemo(() => {
    const term = searchTerm.toLowerCase().trim();
    return ingredients.filter(ingredient => {
      const isAlreadySelected = selectedIngredients.some(
        si => si.ingredientId === ingredient.id.toString()
      );
      if (term) {
        return ingredient.name.toLowerCase().includes(term) && !isAlreadySelected;
      }
      return !isAlreadySelected;
    });
  }, [ingredients, searchTerm, selectedIngredients]);

  const handleAddIngredient = () => {
    if (!selectedIngredientForAdd) return;
    
    const newIngredient = {
      ingredientId: selectedIngredientForAdd.id.toString(),
      quantity: isQuantityRequired ? quantity : '',
      unitId: unitId
    };
    
    const newIngredients = [...selectedIngredients, newIngredient];
    setSelectedIngredients(newIngredients);
    onChange({ target: { name, value: newIngredients } });
    
    resetAddForm();
  };

  const removeIngredient = (index) => {
    const newIngredients = selectedIngredients.filter((_, i) => i !== index);
    setSelectedIngredients(newIngredients);
    onChange({ target: { name, value: newIngredients } });
  };

  const updateIngredient = (index, field, fieldValue) => {
    const newIngredients = selectedIngredients.map((ingredient, i) => {
      if (i === index) {
        const updatedIngredient = { 
          ...ingredient, 
          [field]: fieldValue 
        };
        if (field === 'unitId') {
          const quantityRequired = checkIfQuantityRequired(fieldValue);
          if (!quantityRequired) {
            updatedIngredient.quantity = '';
          }
        }
        return updatedIngredient;
      }
      return ingredient;
    });
    
    setSelectedIngredients(newIngredients);
    onChange({ target: { name, value: newIngredients } });
  };

  const renderIngredientInfo = (ingredient) => (
    <div className="grid grid-cols-2 gap-1 text-xs text-muted-foreground">
      <div>Ккал: {ingredient.calories}/100г</div>
      <div>Белки: {ingredient.proteins}г</div>
      <div>Жиры: {ingredient.fats}г</div>
      <div>Углеводы: {ingredient.carbohydrates}г</div>
    </div>
  );

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <Input
          placeholder="Введите название ингредиента..."
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            if (!e.target.value.trim()) {
              setSelectedIngredientForAdd(null);
            }
          }}
        />

        {filteredIngredients.length > 0 && !selectedIngredientForAdd && (
          <div>
            <p className="text-sm text-muted-foreground mb-1">
              {searchTerm ? 'Найдено:' : 'Все доступные ингредиенты:'} {filteredIngredients.length}
            </p>
            <ScrollArea className="h-60 border rounded-xl">
              <div className="p-2 space-y-1">
                {filteredIngredients.map(ingredient => (
                  <Button
                    key={ingredient.id}
                    type="button"
                    variant="ghost"
                    className="w-full justify-start h-auto p-2"
                    onClick={() => {
                      setSelectedIngredientForAdd(ingredient);
                      setSearchTerm(ingredient.name);
                    }}
                  >
                    <div className="text-left">
                      <div className="font-medium">{ingredient.name}</div>
                      {ingredient.calories && renderIngredientInfo(ingredient)}
                    </div>
                  </Button>
                ))}
              </div>
            </ScrollArea>
          </div>
        )}

        {selectedIngredientForAdd && (
          <Card className="rounded-xl">
            <CardContent className="p-4 space-y-3">
              <div>
                <div className="font-medium">Выбран: {selectedIngredientForAdd.name}</div>
                <Button
                  type="button"
                  variant="link"
                  size="sm"
                  onClick={resetAddForm}
                  className="h-auto p-0"
                >
                  Изменить выбор
                </Button>
              </div>

              <div className="space-y-2">
                <Label>Единица измерения</Label>
                <Select value={unitId} onValueChange={setUnitId}>
                  <SelectTrigger className="rounded-xl">
                    <SelectValue placeholder="Выберите единицу измерения..." />
                  </SelectTrigger>
                  <SelectContent className="bg-white">
                    {units.map(unit => (
                      <SelectItem key={unit.id} value={unit.id.toString()}>
                        {unit.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {unitId && !isQuantityRequired && (
                <p className="text-sm text-muted-foreground">
                  Для этой единицы измерения количество не требуется
                </p>
              )}

              {isQuantityRequired && (
                <div className="space-y-2">
                  <Label>Количество</Label>
                  <Input
                    type="number"
                    value={quantity}
                    onChange={(e) => setQuantity(e.target.value)}
                    placeholder="0.0"
                    step="0.1"
                    min="0"
                    required
                  />
                </div>
              )}

              <Button
                type="button"
                onClick={handleAddIngredient}
                disabled={!unitId || (isQuantityRequired && !quantity)}
                className="w-full"
                variant="violet"
              >
                + Добавить ингредиент
              </Button>
            </CardContent>
          </Card>
        )}
      </div>

      <div>
        <h4 className="font-medium mb-3">
          Ингредиенты в рецепте ({selectedIngredients.length}):
        </h4>
        
        {selectedIngredients.length === 0 ? (
          <div className="text-center py-8 text-muted-foreground border rounded-xl">
            <p className="mb-2">Нет добавленных ингредиентов</p>
            <p className="text-sm">Используйте поиск выше для добавления</p>
          </div>
        ) : (
          <div className="space-y-3">
            {selectedIngredients.map((ingredient, index) => {
              const selectedIngredient = ingredients.find(
                i => i.id.toString() === ingredient.ingredientId
              );
              const quantityRequired = checkIfQuantityRequired(ingredient.unitId);

              return (
                <Card key={index} className="rounded-xl">
                  <CardContent className="p-4 space-y-1">
                    <div className="flex items-center justify-between">
                      <div className="font-medium text-md">
                        {selectedIngredient?.name || 'Не выбран'}
                      </div>
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        onClick={() => removeIngredient(index)}
                        className="text-destructive hover:text-destructive/80"
                      >
                        ✕
                      </Button>
                    </div>
                    
                    <div className="space-y-2">
                      <Label>Единица измерения</Label>
                      <Select
                        value={ingredient.unitId || ''}
                        onValueChange={(value) => updateIngredient(index, 'unitId', value)}
                      >
                        <SelectTrigger className="rounded-xl">
                          <SelectValue placeholder="Выберите единицу..." />
                        </SelectTrigger>
                        <SelectContent>
                          {units.map(unit => (
                            <SelectItem key={unit.id} value={unit.id.toString()}>
                              {unit.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    {quantityRequired && (
                      <div className="space-y-2">
                        <Label>Количество</Label>
                        <Input
                          type="number"
                          value={ingredient.quantity || ''}
                          onChange={(e) => updateIngredient(index, 'quantity', e.target.value)}
                          placeholder="0.0"
                          step="0.1"
                          min="0"
                          required
                        />
                      </div>
                    )}

                    {selectedIngredient && renderIngredientInfo(selectedIngredient)}
                  </CardContent>
                </Card>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};

export default IngredientPicker;