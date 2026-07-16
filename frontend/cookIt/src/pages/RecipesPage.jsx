import React, { useState, useEffect } from "react";
import { useAuth } from "../contexts/AuthContext";
import { useNavigate, Link } from "react-router-dom";
import RecipeCard from "../components/RecipeCard";
import { API_BASE_URL } from '../config/settings';
import { useDebounce } from "../hooks/useDebounce";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Loader2, Plus } from "lucide-react";
import Pagination from "../components/Pagination";
import FiltersSheet from "../components/FiltersSheet";

const RecipesPage = () => {
  const { accessToken, user, refreshAccessToken } = useAuth(); 
  const navigate = useNavigate();
  const [recipes, setRecipes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionError, setActionError] = useState(null);

  const [editingRecipe, setEditingRecipe] = useState(null);
  const [modalOpen, setModalOpen] = useState(false);

  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 9;

  const [filters, setFilters] = useState(() => {
    const savedFilters = localStorage.getItem('recipeFilters');
    return savedFilters ? JSON.parse(savedFilters) : {
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
  });

  const [searchInput, setSearchInput] = useState(filters.searchText || '');
  
  const debouncedSearchText = useDebounce(searchInput, 500);

  useEffect(() => {
    localStorage.setItem('recipeFilters', JSON.stringify(filters));
  }, [filters]);

  const [dishTypes, setDishTypes] = useState([]);
  const [equipments, setEquipments] = useState([]);
  const [ingredients, setIngredients] = useState([]);

  useEffect(() => {
    if (debouncedSearchText !== undefined && debouncedSearchText !== filters.searchText) {
      setFilters(prev => ({ ...prev, searchText: debouncedSearchText }));
      setCurrentPage(1);
    }
  }, [debouncedSearchText]);

  useEffect(() => {
    if (accessToken) {
      const timer = setTimeout(() => {
        fetchRecipes();
      }, 100);
      
      return () => clearTimeout(timer);
    }
  }, [filters, currentPage, accessToken]);

  useEffect(() => {
    const fetchFilterData = async () => {
      try {
        const dishTypesResponse = await fetch(`${API_BASE_URL}/api/dishtypes`, {
          headers: { Authorization: `Bearer ${accessToken}` }
        });
        const dishTypesData = await dishTypesResponse.json();
        setDishTypes(dishTypesData);

        const equipmentsResponse = await fetch(`${API_BASE_URL}/api/equipments`, {
          headers: { Authorization: `Bearer ${accessToken}` }
        });
        const equipmentsData = await equipmentsResponse.json();
        setEquipments(equipmentsData);

        const ingredientsResponse = await fetch(`${API_BASE_URL}/api/ingredients`, {
          headers: { Authorization: `Bearer ${accessToken}` }
        });
        const ingredientsData = await ingredientsResponse.json();
        setIngredients(ingredientsData);
      } catch (error) {
        console.error("Ошибка загрузки данных для фильтров", error);
      }
    };

    if (accessToken) {
      fetchFilterData();
    }
  }, [accessToken]);

  const applyFilters = () => {
    setCurrentPage(1);
    localStorage.setItem('recipeFilters', JSON.stringify(filters));
  };

  const resetFilters = () => {
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
    
    setFilters(defaultFilters);
    setSearchInput(''); 
    setCurrentPage(1);
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
  };

  const fetchRecipes = async () => {
    setLoading(true);
    setError(null);
    setActionError(null);
    
    try {
      const params = new URLSearchParams();

      params.append('PageNumber', currentPage);
      params.append('PageSize', pageSize);
      
      if (filters.searchText) {
        params.append('SearchText', filters.searchText);
      }
      
      if (filters.minCookingTimeWithUser) params.append('minCookingTimeWithUser', filters.minCookingTimeWithUser);
      if (filters.maxCookingTimeWithUser) params.append('maxCookingTimeWithUser', filters.maxCookingTimeWithUser);
      if (filters.minCookingTimeWithoutUser) params.append('minCookingTimeWithoutUser', filters.minCookingTimeWithoutUser);
      if (filters.maxCookingTimeWithoutUser) params.append('maxCookingTimeWithoutUser', filters.maxCookingTimeWithoutUser);
      
      if (filters.minSpicinessLevel) params.append('minSpicinessLevel', filters.minSpicinessLevel);
      if (filters.maxSpicinessLevel) params.append('maxSpicinessLevel', filters.maxSpicinessLevel);
      if (filters.minDifficultyLevel) params.append('minDifficultyLevel', filters.minDifficultyLevel);
      if (filters.maxDifficultyLevel) params.append('maxDifficultyLevel', filters.maxDifficultyLevel);
      
      filters.dishTypeIds.forEach(id => params.append('dishTypeIds', id));
      filters.equipmentIds.forEach(id => params.append('equipmentIds', id));
      filters.ingredientIds.forEach(id => params.append('ingredientIds', id));
      
      const url = `${API_BASE_URL}/api/recipes?${params.toString()}`;
      
      const response = await fetch(url, {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.status === 401) {
        navigate("/login");
        return;
      }

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || `Ошибка: ${response.status}`);
      }

      const paginationHeader = response.headers.get('X-Pagination');
      if (paginationHeader) {
        const paginationData = JSON.parse(paginationHeader);
        console.log("Pagination data:", paginationData);
        setTotalPages(paginationData.TotalPages || 1); 
      }

      const data = await response.json();
      setRecipes(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error("Ошибка загрузки рецептов", error);
      setError(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (e) => {
    const { name, value, type } = e.target;
    
    if (type === 'multiselect') {
      setFilters(prev => ({ ...prev, [name]: value }));
    } else {
      const val = value === '' ? null : value;
      setFilters(prev => ({ ...prev, [name]: val }));
    }
  };

  const handleSearchChange = (e) => {
    const value = e.target.value;
    setSearchInput(value); 
  };

  useEffect(() => {
    const checkAuth = async () => {
      if (!accessToken) {
        try {
          await refreshAccessToken();
        } catch (error) {
          console.log("Не удалось обновить токен", error);
          navigate("/login");
        }
      }
    };

    checkAuth();
  }, [accessToken, navigate]);

  const handleDelete = async (id) => {
    setActionError(null);
    if (!window.confirm("Вы уверены, что хотите удалить этот рецепт?")) return;

    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${id}`, {
        method: "DELETE",
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.status === 401) {
        navigate("/login");
        return;
      }

      if (!response.ok) {
        throw new Error("Ошибка при удалении рецепта");
      }

      await fetchRecipes();
    } catch (error) {
      console.error("Ошибка при удалении рецепта", error);
      setActionError(error.message);
    }
  };

  const handleToggleFavorite = async (recipeId, isFavorite) => {
    try {
      const url = `${API_BASE_URL}/api/favorites/${recipeId}`;
      const method = isFavorite ? 'DELETE' : 'POST';

      const response = await fetch(url, {
        method: method,
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (!response.ok) {
        throw new Error('Ошибка при обновлении избранного');
      }

      setRecipes(prevRecipes => prevRecipes.map(r => 
        r.id === recipeId ? { ...r, isFavorite: !isFavorite } : r
      ));

    } catch (error) {
      console.error(error);
    }
  };

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
      <label className="block mb-2 font-medium">{label}</label>
      
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

  if (!accessToken) {
    return <div>Проверка авторизации...</div>;
  }

  if (loading) {
    return (
      <div className="min-h-screen font-sans flex flex-col">
        <div className="flex-1 flex items-center justify-center">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-purple-900"></div>
            <p className="mt-4 text-gray-600">Загрузка рецептов...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-5 text-center text-red-500">
        Ошибка: {error}
        <button 
          onClick={fetchRecipes}
          className="ml-2 bg-blue-500 text-white px-3 py-1 rounded"
        >
          Повторить
        </button>
      </div>
    );
  }

return (
  <div className="flex flex-col min-h-full">
    <div className="flex-shrink-0 space-y-8">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Все рецепты</h1>
        <p className="text-muted-foreground mt-3 text-base">
          Найдите блюдо по душе
        </p>
      </div>
      <div className="flex flex-col sm:flex-row gap-4 items-start sm:items-center">
        <div className="flex-1 w-full">
          <Input
            type="text"
            placeholder="Поиск по названию или описанию..."
            value={searchInput}
            onChange={handleSearchChange}
            className="w-full"
          />
        </div>
        <div className="flex gap-2 w-full sm:w-auto flex-shrink-0">
        <FiltersSheet
          filters={filters}
          onApplyFilters={(newFilters) => {
            setFilters(newFilters);
            setCurrentPage(1);
            localStorage.setItem('recipeFilters', JSON.stringify(newFilters));
          }}
          dishTypes={dishTypes}
          equipments={equipments}
          ingredients={ingredients}
        />
          <Button variant="violet" asChild>
            <Link to="/recipes/create">
              <Plus className="mr-2 h-4 w-4" />
              Добавить
            </Link>
          </Button>
        </div>
      </div>

      {actionError && (
        <div className="p-3 bg-red-100 text-red-700 rounded text-center">
          {actionError}
        </div>
      )}
    </div>

    <div className="flex-1 mt-8">
      {loading && (
        <div className="flex items-center justify-center h-full">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      )}

      {error && (
        <div className="text-center py-12">
          <p className="text-red-500 mb-4">{error}</p>
          <Button onClick={fetchRecipes} variant="outline">
            Повторить
          </Button>
        </div>
      )}

      {!loading && !error && (
        <>
          {recipes.length > 0 ? (
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-6 gap-6 auto-rows-fr">
              {recipes.map((recipe, index) => {
                const sizePattern = [4, 2, 2, 2, 2, 3, 3, 2, 4];
                const size = sizePattern[index % sizePattern.length];
                
                const sizeClasses = {
                  2: "lg:col-span-2",
                  3: "lg:col-span-3",
                  4: "lg:col-span-4",
                };

                const isAdmin = user?.roles?.includes("Admin");
                const isCreator = recipe.creatorId === user?.id;
                const showActions = isAdmin || isCreator;

                return (
                  <div
                    key={recipe.id}
                    className={sizeClasses[size]}
                  >
                    <RecipeCard
                      recipe={recipe}
                      onToggleFavorite={handleToggleFavorite}
                      showActions={showActions}
                      onEdit={() => {
                        setEditingRecipe(recipe);
                        setModalOpen(true);
                      }}
                      onDelete={() => handleDelete(recipe.id)}
                    />
                  </div>
                );
              })}
            </div>
          ) : (
            <div className="text-center py-12">
              <p className="text-muted-foreground mb-4">Рецептов не найдено</p>
              <Button onClick={fetchRecipes} variant="outline">
                Обновить
              </Button>
            </div>
          )}
        </>
      )}
    </div>

    {!loading && !error && recipes.length > 0 && (
      <div className="flex-shrink-0 mt-8">
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={handlePageChange}
        />
      </div>
    )}
  </div>
);
};

export default RecipesPage;