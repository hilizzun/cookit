import React, { useState, useEffect, useRef } from "react";
import { useAuth } from "../contexts/AuthContext";
import IngredientPicker from "./IngredientPicker";
import VisualRatingSelect from './VisualRatingSelect';
import { FaPepperHot, FaCheck, FaClock } from "react-icons/fa";
import { GiWhisk } from "react-icons/gi";
import { useNavigate } from 'react-router-dom';
import { API_BASE_URL } from '../config/settings';
import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Image from '@tiptap/extension-image';
import { useDebounce } from "../hooks/useDebounce";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import MultiSelect from "./ui/multi-select";
import { Bold, Italic, List, Heading2, Image as ImageIcon } from "lucide-react";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../components/ui/select";
import { Card, CardContent } from "../components/ui/card";

const RecipeForm = ({ recipe, onSave, onCancel }) => {
  const { accessToken, user } = useAuth();
  const navigate = useNavigate();
  const isModerator = user?.roles?.includes('Admin') || user?.roles?.includes('Moderator');

  const [ingredientSearch, setIngredientSearch] = useState('');
  const [filteredIngredients, setFilteredIngredients] = useState([]);
  const debouncedSearch = useDebounce(ingredientSearch, 300);

  const [formData, setFormData] = useState({
    id: recipe?.id || "",
    name: recipe?.name || "",
    shortDescription: recipe?.shortDescription || "",
    fullDescription: recipe?.fullDescription || "",
    ingredients: [],
    specialEquipment: recipe?.specialEquipment || [],
    cookingTimeWithUser: recipe?.cookingTimeWithUser || "",
    cookingTimeWithoutUser: recipe?.cookingTimeWithoutUser || "",
    spicinessLevel: recipe?.spicinessLevel || 1,
    difficultyLevel: recipe?.difficultyLevel || 1,
    dishType: recipe?.dishType?.id || recipe?.dishType || "",
    servings: recipe?.servings || "",
    image: null
  });

  const [imagePreview, setImagePreview] = useState("");
  const [allIngredients, setAllIngredients] = useState([]);
  const [allEquipment, setAllEquipment] = useState([]);
  const [allDishTypes, setAllDishTypes] = useState([]);
  const [allUnits, setAllUnits] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const ingredients = recipe?.recipeIngredients?.map(ri => ({
      ingredientId: ri.ingredientId.toString(),
      quantity: ri.quantity?.toString() || "",
      unitId: ri.unitId?.toString() || ""
    })) || [];

    const equipment = recipe?.recipeEquipments?.map(re => re.equipmentId) || [];

    if (recipe) {
      setFormData({
        id: recipe?.id || "",
        name: recipe?.name || "",
        shortDescription: recipe?.shortDescription || "",
        fullDescription: recipe?.fullDescription || "",
        ingredients,
        specialEquipment: equipment,
        cookingTimeWithUser: recipe?.cookingTimeWithUser || "",
        cookingTimeWithoutUser: recipe?.cookingTimeWithoutUser || "",
        spicinessLevel: recipe?.spicinessLevel || 1,
        difficultyLevel: recipe?.difficultyLevel || 1,
        dishType: recipe.dishType?.id ? String(recipe.dishType.id) : "",
        servings: recipe?.servings || "",
        image: null
      });

      if (recipe?.imageUrl) {
        setImagePreview(recipe.imageUrl);
      }
    } else {
      setFormData({
        name: "",
        shortDescription: "",
        fullDescription: "",
        ingredients: [],
        specialEquipment: [],
        cookingTimeWithUser: "",
        cookingTimeWithoutUser: "",
        spicinessLevel: 1,
        difficultyLevel: 1,
        dishType: "",
        servings: "",
        image: null
      });
      setImagePreview("");
    }
  }, [recipe]);

  useEffect(() => {
    if (!allIngredients) return;
    
    if (!debouncedSearch.trim()) {
      setFilteredIngredients(allIngredients);
    } else {
      const filtered = allIngredients.filter(ingredient =>
        ingredient.name.toLowerCase().includes(debouncedSearch.toLowerCase())
      );
      setFilteredIngredients(filtered);
    }
  }, [debouncedSearch, allIngredients]);

  const getSubmitButtonText = () => {
    if (!recipe) {
      return isModerator ? "Сохранить" : "Отправить на проверку";
    } else {
      if (recipe.isApproved === false) {
        return "Отправить повторно";
      } else if (recipe.isApproved === null) {
        return "Обновить";
      } else {
        return "Сохранить";
      }
    }
  };
  
  const submitButtonText = getSubmitButtonText();

  useEffect(() => {
    const fetchData = async () => {
      try {
        const responses = await Promise.all([
          fetch(`${API_BASE_URL}/api/ingredients`, {
            headers: { Authorization: `Bearer ${accessToken}` }
          }),
          fetch(`${API_BASE_URL}/api/equipments`, {
            headers: { Authorization: `Bearer ${accessToken}` }
          }),
          fetch(`${API_BASE_URL}/api/dishtypes`, {
            headers: { Authorization: `Bearer ${accessToken}` }
          }),
          fetch(`${API_BASE_URL}/api/units`, {
            headers: { Authorization: `Bearer ${accessToken}` }
          })
        ]);

        const [ingredients, equipment, dishTypes, units] = await Promise.all(
          responses.map(res => res.json())
        );

        setAllIngredients(ingredients);
        setAllEquipment(equipment);
        setAllDishTypes(dishTypes);
        setAllUnits(units);
        setFilteredIngredients(ingredients); 
      } catch (error) {
        console.error("Error loading data", error);
        setError("Ошибка загрузки данных");
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [accessToken]);

  const handleChange = (e) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'number' ? Number(value) : value
    }));
  };

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setFormData(prev => ({ ...prev, image: file }));
      setImagePreview(URL.createObjectURL(file));
    }
  };

  const editor = useEditor({
    extensions: [
      StarterKit,
      Image.configure({
        inline: true,
        allowBase64: true,
      }),
    ],
    content: formData.fullDescription,
    onUpdate: ({ editor }) => {
      setFormData(prev => ({ ...prev, fullDescription: editor.getHTML() }));
    },
  });

  const handleImageUpload = async () => {
    const input = document.createElement('input');
    input.setAttribute('type', 'file');
    input.setAttribute('accept', 'image/*');
    input.click();

    input.onchange = async () => {
      const file = input.files[0];
      if (!file) return;

      const formData = new FormData();
      formData.append('file', file);

      try {
        const response = await fetch(`${API_BASE_URL}/api/images/upload`, { 
          method: 'POST',
          headers: { Authorization: `Bearer ${accessToken}` },
          body: formData
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.Error || 'Ошибка загрузки изображения');
        }

        const result = await response.json();
        const imageKey = result.key; 
        const previewResponse = await fetch(`${API_BASE_URL}/api/images/preview/${imageKey}`, {
          headers: { Authorization: `Bearer ${accessToken}` }
        });

        if (!previewResponse.ok) {
          const errorData = await previewResponse.json();
          throw new Error(errorData.Error || 'Ошибка получения URL изображения');
        }

        const previewResult = await previewResponse.json();
        const imageUrl = previewResult.url;

        if (editor) {
          const proxyUrl = `${API_BASE_URL}/api/images/preview-proxy/${imageKey}`;
          editor.chain().focus().setImage({ src: proxyUrl }).run();
        }
      } catch (err) {
        console.error('Ошибка загрузки изображения:', err);
        setError(err.message);
      }
    };
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const filteredIngredientList = formData.ingredients
      .filter(ing => ing.ingredientId)
      .map(ing => ({
        ingredientId: parseInt(ing.ingredientId),
        quantity: ing.quantity ? parseFloat(ing.quantity) : null,
        unitId: ing.unitId ? parseInt(ing.unitId) : null
      }));

    const formattedData = {
      name: formData.name,
      shortDescription: formData.shortDescription,
      fullDescription: formData.fullDescription,
      dishTypeId: parseInt(formData.dishType),
      recipeIngredients: filteredIngredientList,
      recipeEquipments: formData.specialEquipment.map(id => parseInt(id)),
      cookingTimeWithUser: parseInt(formData.cookingTimeWithUser),
      cookingTimeWithoutUser: parseInt(formData.cookingTimeWithoutUser),
      spicinessLevel: parseInt(formData.spicinessLevel),
      difficultyLevel: parseInt(formData.difficultyLevel),
      servings: formData.servings ? parseInt(formData.servings) : null,
      id: formData.id ? parseInt(formData.id) : null,
      image: formData.image
    };

    onSave(formattedData);
  };

  if (isLoading) {
    return (
      <div className="flex-1 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
          <p className="mt-4 text-gray-600">Загрузка данных...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-5 text-center text-red-500">
        Ошибка: {error}
        <Button onClick={() => navigate('/recipes')} variant="outline" className="ml-2">
          Назад к рецептам
        </Button>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {recipe && (
        <div className="space-y-4">
          {recipe.isApproved === false && recipe.rejectionComment && (
            <Card className="border-red-200 bg-red-50">
              <CardContent className="p-4">
                <h4 className="font-semibold text-red-800 mb-2">Рецепт отклонен</h4>
                <p className="text-red-700">Причина: {recipe.rejectionComment}</p>
              </CardContent>
            </Card>
          )}
          
          {recipe.isApproved === null && (
            <Card className="border-yellow-200 bg-yellow-50">
              <CardContent className="p-4">
                <div className="flex items-center">
                  <FaClock className="text-yellow-500 mr-2" />
                  <span className="font-medium text-yellow-800">Рецепт находится на проверке</span>
                </div>
                <p className="text-yellow-700 text-sm mt-1">
                  После редактирования рецепт будет отправлен на повторную проверку
                </p>
              </CardContent>
            </Card>
          )}
        </div>
      )}

      <div className="space-y-2">
        <Label htmlFor="name">Название</Label>
        <Input
          id="name"
          name="name"
          value={formData.name}
          onChange={handleChange}
          required
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="shortDescription">Краткое описание</Label>
        <Input
          id="shortDescription"
          name="shortDescription"
          value={formData.shortDescription}
          onChange={handleChange}
          required
        />
      </div>

      <div className="space-y-2">
        <Label>Описание приготовления</Label>
        <div className="border rounded-xl overflow-hidden">
          <div className="flex gap-1 p-2 border-b bg-muted/30">
            <Button
              type="button"
              variant={editor?.isActive('bold') ? 'violet' : 'ghost'}
              size="sm"
              onClick={() => editor?.chain().focus().toggleBold().run()}
              className="px-2"
            >
              <Bold className="h-4 w-4" />
            </Button>
            <Button
              type="button"
              variant={editor?.isActive('italic') ? 'violet' : 'ghost'}
              size="sm"
              onClick={() => editor?.chain().focus().toggleItalic().run()}
              className="px-2"
            >
              <Italic className="h-4 w-4" />
            </Button>
            <Button
              type="button"
              variant={editor?.isActive('bulletList') ? 'violet' : 'ghost'}
              size="sm"
              onClick={() => editor?.chain().focus().toggleBulletList().run()}
              className="px-2"
            >
              <List className="h-4 w-4" />
            </Button>
            <Button
              type="button"
              variant={editor?.isActive('heading', { level: 2 }) ? 'violet' : 'ghost'}
              size="sm"
              onClick={() => editor?.chain().focus().setHeading({ level: 2 }).run()}
              className="px-2"
            >
              <Heading2 className="h-4 w-4" />
            </Button>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={handleImageUpload}
              className="px-2"
            >
              <ImageIcon className="h-4 w-4" />
            </Button>
          </div>
          <EditorContent editor={editor} className="p-2 min-h-[200px]" />
        </div>
      </div>

      <div>
        <label className="block text-lg font-medium text-gray-900 mb-1">
            Изображение рецепта
        </label>
        <input
          type="file"
          accept="image/*"
          onChange={handleImageChange}
          className="block w-full text-sm text-gray-500
                    file:mr-4 file:py-2 file:px-4
                    file:rounded-full file:border-0
                    file:text-sm file:font-semibold
                    file:bg-violet-50 file:text-violet-900
                    hover:file:bg-violet-100"
        />

        {imagePreview && (
          <div className="mt-2">
            <img
              src={imagePreview}
              alt="Превью"
              className="h-32 object-contain rounded-lg"
              onError={(e) => {
                e.target.style.display = 'none';
              }}
            />
          </div>
        )}
      </div>

      <div className="space-y-2">
        <div className="flex justify-between items-center">
          <Label>Ингредиенты</Label>
          {ingredientSearch && (
            <span className="text-sm text-muted-foreground">
              Найдено: {filteredIngredients.length}
            </span>
          )}
        </div>
        <IngredientPicker
          ingredients={filteredIngredients}
          units={allUnits}
          value={formData.ingredients}
          onChange={handleChange}
          name="ingredients"
        />
      </div>

      <div className="space-y-2">
        <Label>Спец. оборудование</Label>
        <MultiSelect
          options={allEquipment}
          value={formData.specialEquipment}
          onChange={handleChange}
          name="specialEquipment"
          placeholder="Выберите оборудование..."
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="cookingTimeWithUser">Время приготовления с участием человека (мин)</Label>
        <Input
          id="cookingTimeWithUser"
          type="number"
          name="cookingTimeWithUser"
          value={formData.cookingTimeWithUser}
          onChange={handleChange}
          required
          min="0"
        />
      </div>

      <div className="space-y-2">
        <Label htmlFor="cookingTimeWithoutUser">Общее время приготовления (мин)</Label>
        <Input
          id="cookingTimeWithoutUser"
          type="number"
          name="cookingTimeWithoutUser"
          value={formData.cookingTimeWithoutUser}
          onChange={handleChange}
          required
          min="0"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="space-y-2">
          <Label>Острота</Label>
          <VisualRatingSelect
            value={formData.spicinessLevel}
            onChange={(value) => setFormData(prev => ({...prev, spicinessLevel: value}))}
            max={5}
            FilledIcon={FaPepperHot}
            OutlinedIcon={FaPepperHot}
            filledClass="text-red-500"
            outlinedClass="text-gray-300"
          />
        </div>

        <div className="space-y-2">
          <Label>Сложность</Label>
          <VisualRatingSelect
            value={formData.difficultyLevel}
            onChange={(value) => setFormData(prev => ({...prev, difficultyLevel: value}))}
            max={5}
            FilledIcon={GiWhisk}
            OutlinedIcon={GiWhisk}
            filledClass="text-green-500"
          />
        </div>
      </div>

      <div className="space-y-2">
        <Label>Тип блюда</Label>
        <Select
          name="dishType"
          value={formData.dishType}
          onValueChange={(value) => setFormData(prev => ({...prev, dishType: value}))}
        >
          <SelectTrigger className="rounded-xl">
            <SelectValue placeholder="Выберите тип блюда" />
          </SelectTrigger>
          <SelectContent className="bg-white ">
            {allDishTypes.map(type => (
              <SelectItem key={type.id} value={type.id.toString()}>
                {type.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-2">
        <Label htmlFor="servings">Количество порций (от 1 до 12)</Label>
        <Input
          id="servings"
          type="number"
          name="servings"
          value={formData.servings}
          onChange={handleChange}
          min="1"
          max="12"
        />
      </div>

      <div className="flex justify-center gap-4 pt-4">
        <Button type="submit" variant="violet" size="lg" className="w-60">
          {submitButtonText}
        </Button>
        <Button type="button" variant="dashedHover" size="lg" className="w-60" onClick={onCancel}>
          Отмена
        </Button>
      </div>
    </form>
  );
};

export default RecipeForm;