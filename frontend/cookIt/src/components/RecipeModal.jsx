import React, { useState, useEffect } from "react";
import { useAuth } from "../contexts/AuthContext";
import MultiSelectCheckbox from "./MultiSelectCheckbox"; 

const RecipeModal = ({ isOpen, onClose, onSave, recipe }) => {
  const { accessToken } = useAuth();
  const [formData, setFormData] = useState({
    id: recipe?.id || "",
    name: recipe?.name || "",
    shortDescription: recipe?.shortDescription || "",
    fullDescription: recipe?.fullDescription || "",
    ingredients: recipe?.ingredients || [],
    specialEquipment: recipe?.specialEquipment || [],
    cookingTimeWithUser: recipe?.cookingTimeWithUser || "",
    cookingTimeWithoutUser: recipe?.cookingTimeWithoutUser || "",
    spicinessLevel: recipe?.spicinessLevel || "",
    difficultyLevel: recipe?.difficultyLevel || "",
    dishType: recipe?.dishType || "",
    servings: recipe?.servings || "",
    image: null
  });

  const [imagePreview, setImagePreview] = useState("");
  const [allIngredients, setAllIngredients] = useState([]);
  const [allEquipment, setAllEquipment] = useState([]);
  const [allDishTypes, setAllDishTypes] = useState([]);

  useEffect(() => {
    if (!isOpen) return;
    
    const ingredients = recipe?.recipeIngredients?.map(ri => ri.ingredientId) || [];
    const equipment = recipe?.recipeEquipments?.map(re => re.equipmentId) || [];

    setFormData({
      id: recipe?.id || "",
      name: recipe?.name || "",
      shortDescription: recipe?.shortDescription || "",
      fullDescription: recipe?.fullDescription || "",
      ingredients,
      specialEquipment: equipment,
      cookingTimeWithUser: recipe?.cookingTimeWithUser || "",
      cookingTimeWithoutUser: recipe?.cookingTimeWithoutUser || "",
      spicinessLevel: recipe?.spicinessLevel || "",
      difficultyLevel: recipe?.difficultyLevel || "",
      dishType: recipe?.dishType?.id || recipe?.dishType || "",
      servings: recipe?.servings || "",
      image: null
    });

    if (recipe?.imageUrl) {
      setImagePreview(recipe.imageUrl);
    }
  }, [isOpen, recipe]);

  useEffect(() => {
    if (!isOpen) return;
    
    const fetchData = async () => {
      try {
        const responses = await Promise.all([
          fetch("https://localhost:7031/api/ingredients", {
            headers: { Authorization: `Bearer ${accessToken}` }
          }),
          fetch("https://localhost:7031/api/equipments", {
            headers: { Authorization: `Bearer ${accessToken}` }
          }),
          fetch("https://localhost:7031/api/dishtypes", {
            headers: { Authorization: `Bearer ${accessToken}` }
          })
        ]);

        const [ingredients, equipment, dishTypes] = await Promise.all(
          responses.map(res => res.json())
        );

        setAllIngredients(ingredients);
        setAllEquipment(equipment);
        setAllDishTypes(dishTypes);
      } catch (error) {
        console.error("Error loading data", error);
      }
    };

    fetchData();
  }, [isOpen, accessToken]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      setFormData(prev => ({ ...prev, image: file }));
      setImagePreview(URL.createObjectURL(file));
    }
  };

const handleSubmit = (e) => {
  e.preventDefault();
  
  const formattedData = {
    name: formData.name,
    shortDescription: formData.shortDescription,
    fullDescription: formData.fullDescription,
    dishTypeId: parseInt(formData.dishType),
    recipeIngredients: formData.ingredients.map(id => parseInt(id)),
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

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-opacity-50 flex justify-center items-center z-50 bg-white">
      <div className="bg-white p-5 rounded-3xl shadow-lg w-full max-w-md max-h-[90vh] overflow-y-auto border-2" style={{ borderColor: "#201469" }}>
        <h2 className="text-gray-900 font-bold text-lg text-center mb-3" style={{ textColor: "#201469" }}>
          {recipe ? "Редактировать рецепт" : "Добавить рецепт"}
        </h2>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <input
            name="name"
            value={formData.name}
            onChange={handleChange}
            placeholder="Название"
            className="border p-2 rounded-xl"
            required
          />

          <textarea
            name="shortDescription"
            value={formData.shortDescription}
            onChange={handleChange}
            placeholder="Краткое описание"
            className="border p-2 rounded-xl"
            required
            rows={2}
          />

          <textarea
            name="fullDescription"
            value={formData.fullDescription}
            onChange={handleChange}
            placeholder="Полное описание"
            className="border p-2 rounded-xl"
            required
            rows={4}
          />
          
          <div>
            <label className="block text-xs font-medium text-gray-700 mb-1">
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
                        file:bg-violet-50 file:text-violet-700
                        hover:file:bg-violet-100"
            />
            
            {imagePreview && (
              <div className="mt-2">
                <img 
                  src={imagePreview} 
                  alt="Превью" 
                  className="h-32 object-contain border rounded-lg"
                  onError={(e) => {
                    e.target.style.display = 'none';
                  }}
                />
              </div>
            )}
          </div>

          <label className="text-sm font-medium text-gray-700">Ингредиенты</label>
          <MultiSelectCheckbox
            options={allIngredients}
            value={formData.ingredients}
            onChange={handleChange}
            name="ingredients"
          />

          <label className="text-sm font-medium text-gray-700">Спец. оборудование</label>
          <MultiSelectCheckbox
            options={allEquipment}
            value={formData.specialEquipment}
            onChange={handleChange}
            name="specialEquipment"
          />

          <input
            type="number"
            name="cookingTimeWithUser"
            value={formData.cookingTimeWithUser}
            onChange={handleChange}
            placeholder="Время с участием человека (мин)"
            className="border p-2 rounded-xl"
            required
            min="0"
          />

          <input
            type="number"
            name="cookingTimeWithoutUser"
            value={formData.cookingTimeWithoutUser}
            onChange={handleChange}
            placeholder="Общее время приготовления (мин)"
            className="border p-2 rounded-xl"
            required
            min="0"
          />

          <input
            type="number"
            name="spicinessLevel"
            value={formData.spicinessLevel}
            onChange={handleChange}
            placeholder="Острота (0-5)"
            className="border p-2 rounded-xl"
            required
            min="0"
            max="5"
          />

          <input
            type="number"
            name="difficultyLevel"
            value={formData.difficultyLevel}
            onChange={handleChange}
            placeholder="Сложность (1-5)"
            className="border p-2 rounded-xl"
            required
            min="1"
            max="5"
          />

          <label className="text-sm font-medium text-gray-700">Тип блюда</label>
          <select
            name="dishType"
            value={formData.dishType}
            onChange={handleChange}
            className="border p-2 rounded-xl"
            required
          >
            <option value="">Выберите тип блюда</option>
            {allDishTypes.map(t => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </select>

          <input
              type="number"
              name="servings"
              value={formData.servings}
              onChange={handleChange}
              placeholder="Количество порций (1-12)"
              className="border p-2 rounded-xl"
              min="1"
              max="12"
            />

          <div className="flex justify-center gap-2 pt-3">
            <button
              type="submit"
              className="text-white p-2 rounded-2xl w-45 transition"
              style={{ backgroundColor: "#201469" }}
            >
              Сохранить
            </button>
            <button
              type="button"
              onClick={onClose}
              className="text-white p-2 rounded-2xl w-45 transition" 
              style={{ backgroundColor: "#A586E2" }}
            >
              Отмена
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default RecipeModal;