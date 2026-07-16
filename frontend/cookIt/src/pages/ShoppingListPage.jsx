import React, { useMemo, useState } from 'react';
import { useShoppingList } from '../contexts/ShoppingListContext';
import { Button } from '../components/ui/button';
import { Card, CardContent } from '../components/ui/card';
import { Separator } from '../components/ui/separator';
import { Badge } from '../components/ui/badge';
import { pdf } from '@react-pdf/renderer';
import ShoppingListPDF from '../components/ShoppingListPDF';
import { Copy, Download, Trash2, Check } from 'lucide-react';

const ShoppingListPage = () => {
  const { recipes, loading, removeRecipe, updateServings, toggleExcludeIngredient } = useShoppingList();
  const [exporting, setExporting] = useState(false);
  const [copied, setCopied] = useState(false);

  // Группировка с переводом в граммы (с учётом порций и исключённых ингредиентов)
  const groupedIngredients = useMemo(() => {
    const map = new Map(); // ключ: `${ingredientId}_${type}`

    for (const recipe of recipes) {
      const factor = recipe.servings / (recipe.originalServings || 1);

      for (const ing of recipe.ingredients) {
        if (ing.isExcluded) continue;

        let key, quantity, unit, name;

        // Штучные ингредиенты – не переводим в граммы
        if (ing.isByPiece) {
          key = `${ing.ingredientId}_piece`;
          quantity = (ing.quantity || 0) * factor;
          unit = ing.unit || 'шт';
          name = ing.name;
        } 
        // Есть коэффициент перевода в граммы – пересчитываем
        else if (ing.conversionToGrams && ing.conversionToGrams > 0) {
          key = `${ing.ingredientId}_grams`;
          quantity = (ing.quantity || 0) * factor * ing.conversionToGrams;
          unit = 'г';
          name = ing.name;
        } 
        // Нет коэффициента – оставляем как есть (оригинальные единицы)
        else {
          key = `${ing.ingredientId}_${ing.unit || 'unit'}`;
          quantity = (ing.quantity || 0) * factor;
          unit = ing.unit || '';
          name = ing.name;
        }

        if (map.has(key)) {
          const existing = map.get(key);
          existing.quantity += quantity;
        } else {
          map.set(key, {
            id: ing.ingredientId,
            name: name,
            quantity: quantity,
            unit: unit,
          });
        }
      }
    }

    return Array.from(map.values());
  }, [recipes]);

  const handleExportPDF = async () => {
    if (!groupedIngredients.length) return;
    setExporting(true);
    try {
      const blob = await pdf(<ShoppingListPDF groupedIngredients={groupedIngredients} />).toBlob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `shopping-list.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Ошибка при создании PDF:', error);
    } finally {
      setExporting(false);
    }
  };

  const copyToClipboard = () => {
    const text = groupedIngredients
      .map(ing => `${ing.name} – ${Math.ceil(ing.quantity)} ${ing.unit}`)
      .join('\n');
    navigator.clipboard.writeText(text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  if (loading) return <div className="flex justify-center p-8">Загрузка...</div>;

  return (
    <div className="container mx-auto p-4 max-w-7xl">
      <div className="flex flex-wrap justify-between items-center gap-4 mb-6">
        <h1 className="text-3xl font-bold tracking-tight">Список покупок</h1>
        <div className="flex gap-2">
          <Button variant="outline" onClick={copyToClipboard} disabled={!groupedIngredients.length}>
            {copied ? <Check className="h-4 w-4 mr-2" /> : <Copy className="h-4 w-4 mr-2" />}
            {copied ? 'Скопировано' : 'Скопировать'}
          </Button>
          <Button variant="outline" onClick={handleExportPDF} disabled={exporting || !groupedIngredients.length}>
            <Download className="h-4 w-4 mr-2" />
            {exporting ? 'Создание...' : 'PDF'}
          </Button>
        </div>
      </div>

      {recipes.length === 0 ? (
        <Card className="p-8 text-center text-muted-foreground">
          Нет добавленных рецептов. Перейдите к рецепту и нажмите «В список покупок».
        </Card>
      ) : (
        <>
          {/* Список выбранных рецептов (с возможностью менять порции и исключать ингредиенты) */}
          <div className="space-y-4 mb-10">
            <h2 className="text-xl font-semibold">Выбранные рецепты</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {recipes.map(recipe => {
                const factor = recipe.servings / (recipe.originalServings || 1);
                return (
                  <Card key={recipe.id} className="overflow-hidden">
                    <CardContent className="p-4">
                      <div className="flex justify-between items-start">
                        <h3 className="text-lg font-medium">{recipe.recipeName}</h3>
                        <Button variant="ghost" size="icon" onClick={() => removeRecipe(recipe.id)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                      <div className="flex items-center gap-2 mt-2">
                        <span className="text-sm text-muted-foreground">Порций:</span>
                        <input
                          type="number"
                          min="1"
                          step="1"
                          value={recipe.servings}
                          onChange={(e) => updateServings(recipe.id, parseFloat(e.target.value))}
                          className="w-16 border rounded-md p-1 text-center"
                        />
                      </div>
                      <Separator className="my-3" />
                      <div className="space-y-1">
                        {recipe.ingredients.map(ing => {
                          const adjustedQuantity = ing.quantity * factor;
                          return (
                            <div key={ing.ingredientId} className="flex items-center gap-2 text-sm">
                              <input
                                type="checkbox"
                                checked={ing.isExcluded}
                                onChange={() => toggleExcludeIngredient(recipe.id, ing.ingredientId)}
                                className="rounded"
                              />
                              <span className={ing.isExcluded ? 'line-through text-muted-foreground' : ''}>
                                {ing.name} – {Math.ceil(adjustedQuantity)} {ing.unit}
                              </span>
                            </div>
                          );
                        })}
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          </div>

          {/* Итоговый список (сгруппированный, пересчитанный в граммы/штуки) */}
          <div className="mt-6">
            <Card>
              <CardContent className="p-5">
                <div className="flex items-center justify-between flex-wrap gap-2 mb-4">
                  <h2 className="text-xl font-semibold flex items-center gap-2">
                    Итоговый список покупок
                    <Badge variant="secondary">{groupedIngredients.length} позиций</Badge>
                  </h2>
                </div>
                {groupedIngredients.length === 0 ? (
                  <p className="text-muted-foreground">
                    Нет активных ингредиентов. Уберите исключения или добавьте рецепты.
                  </p>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-2">
                    {groupedIngredients.map(ing => (
                      <div key={ing.id} className="flex justify-between items-baseline border-b border-gray-100 py-1">
                        <span className="text-foreground">{ing.name}</span>
                        <span className="text-muted-foreground text-sm font-mono whitespace-nowrap">
                          {Math.ceil(ing.quantity)} {ing.unit}
                        </span>
                      </div>
                    ))}
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </>
      )}
    </div>
  );
};

export default ShoppingListPage;