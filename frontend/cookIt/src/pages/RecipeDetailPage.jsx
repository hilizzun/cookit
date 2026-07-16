import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import VisualRating from '../components/VisualRating';
import VisualRatingSelect from '../components/VisualRatingSelect';
import InterestingFactWidget from '../components/InterestingFactWidget';
import { useShoppingList } from '../contexts/ShoppingListContext';
import { API_BASE_URL } from '../config/settings';
import { Button } from '../components/ui/button';
import { Card, CardContent } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../components/ui/tabs';
import { Avatar, AvatarImage, AvatarFallback } from '../components/ui/avatar';
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from '../components/ui/collapsible';
import { Textarea } from '../components/ui/textarea';
import { Edit2, Trash2, Reply, X, Check, Loader2, Flag, ShoppingCart, FileDown } from 'lucide-react';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '../components/ui/alert-dialog';
import {
  FaStar,
  FaRegStar,
  FaPepperHot,
  FaHeart,
  FaRegHeart,
  FaCheck,
  FaTimes,
} from 'react-icons/fa';
import { GiWhisk} from 'react-icons/gi';
import { ChevronDown } from 'lucide-react';
import { pdf } from '@react-pdf/renderer';
import RecipePDF from '../components/RecipePDF';
import UserAchievementsTooltip from '../components/UserAchievementsTooltip';

const RecipeDetailPage = () => {
  const { id } = useParams();
  const { accessToken, user, userAvatar } = useAuth();
  const navigate = useNavigate();
  const [recipe, setRecipe] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [isFavorite, setIsFavorite] = useState(false);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [rejectModalOpen, setRejectModalOpen] = useState(false);
  const [rejectionComment, setRejectionComment] = useState('');
  const [ratingSummary, setRatingSummary] = useState({
    averageRating: 0,
    totalRatings: 0,
    userRating: null,
    ratingDistribution: {},
  });
  const [canRate, setCanRate] = useState(false);
  const [activeNutritionTab, setActiveNutritionTab] = useState('serving');
  const [isOpen, setIsOpen] = useState(false);

  const [comments, setComments] = useState([]);
  const [commentsLoading, setCommentsLoading] = useState(false);
  const [newComment, setNewComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [editingCommentId, setEditingCommentId] = useState(null);
  const [editingContent, setEditingContent] = useState('');
  const [replyingToId, setReplyingToId] = useState(null);
  const [replyContent, setReplyContent] = useState('');
  const [deletingCommentId, setDeletingCommentId] = useState(null);
  const [commentError, setCommentError] = useState(null);
  const [showComments, setShowComments] = useState(true);
  const [complaintDialogOpen, setComplaintDialogOpen] = useState(false);
  const [complaintCommentId, setComplaintCommentId] = useState(null);
  const [complaintReason, setComplaintReason] = useState('');
  const [exporting, setExporting] = useState(false);
  const { addRecipeToList, removeRecipeByRecipeId, isRecipeInList } = useShoppingList();
  const isInShoppingList = isRecipeInList(recipe?.id);

  const fetchRecipe = useCallback(async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${id}`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (response.status === 401) {
        navigate('/login');
        return;
      }
      if (!response.ok) throw new Error('Ошибка загрузки рецепта');
      const data = await response.json();
      setRecipe(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, [id, accessToken, navigate]);

  useEffect(() => {
    fetchRecipe();
  }, [fetchRecipe]);

  useEffect(() => {
    const checkFavoriteStatus = async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/favorites/${id}`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (response.ok) {
          const favoriteStatus = await response.json();
          setIsFavorite(favoriteStatus);
        }
      } catch (error) {
        console.error('Ошибка при проверке избранного:', error);
      }
    };
    if (recipe) checkFavoriteStatus();
  }, [id, accessToken, recipe]);

  useEffect(() => {
    const fetchRatingData = async () => {
      try {
        const summaryResponse = await fetch(`${API_BASE_URL}/api/recipes/${id}/ratings`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (summaryResponse.ok) {
          const summaryData = await summaryResponse.json();
          setRatingSummary(summaryData);
        }
        const canRateResponse = await fetch(`${API_BASE_URL}/api/recipes/${id}/ratings/can-rate`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (canRateResponse.ok) {
          const canRateData = await canRateResponse.json();
          setCanRate(canRateData.canRate);
        }
      } catch (error) {
        console.error('Ошибка при загрузке рейтинга:', error);
      }
    };
    if (recipe) fetchRatingData();
  }, [id, accessToken, recipe]);

  const handleRateRecipe = async (ratingValue) => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${id}/ratings`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ recipeId: parseInt(id), value: ratingValue }),
      });
      if (response.ok) {
        const summaryResponse = await fetch(`${API_BASE_URL}/api/recipes/${id}/ratings`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (summaryResponse.ok) {
          const summaryData = await summaryResponse.json();
          setRatingSummary(summaryData);
        }
      } else {
        const errorData = await response.json();
        alert(errorData.message || 'Ошибка при оценке рецепта');
      }
    } catch (error) {
      console.error('Ошибка при оценке рецепта:', error);
    }
  };

  const handleRemoveRating = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${id}/ratings`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (response.ok) {
        const summaryResponse = await fetch(`${API_BASE_URL}/api/recipes/${id}/ratings`, {
          headers: { Authorization: `Bearer ${accessToken}` },
        });
        if (summaryResponse.ok) {
          const summaryData = await summaryResponse.json();
          setRatingSummary(summaryData);
        }
      }
    } catch (error) {
      console.error('Ошибка при удалении оценки:', error);
    }
  };

  const handleToggleShoppingList = () => {
  if (isInShoppingList) {
    removeRecipeByRecipeId(recipe.id);
  } else {
    addRecipeToList(recipe, recipe.servings);
  }
  };

  const handleToggleFavorite = async () => {
    try {
      const method = isFavorite ? 'DELETE' : 'POST';
      const response = await fetch(`${API_BASE_URL}/api/favorites/${id}`, {
        method,
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (response.ok) {
        setIsFavorite(!isFavorite);
      } else {
        throw new Error('Ошибка при обновлении избранного');
      }
    } catch (error) {
      console.error(error);
    }
  };

  const fetchComments = useCallback(async () => {
    if (!recipe?.id) return;
    setCommentsLoading(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${recipe.id}/comments?pageSize=100`, {
        headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
      });
      if (!response.ok) throw new Error('Ошибка загрузки комментариев');
      const data = await response.json();
      const tree = buildCommentTree(data);
      
      const filterDeletedComments = (comments) => {
        return comments.reduce((acc, comment) => {
          const filteredReplies = filterDeletedComments(comment.replies);
          if (comment.isDeleted && filteredReplies.length === 0) {
            return acc; 
          }
          acc.push({ ...comment, replies: filteredReplies });
          return acc;
        }, []);
      };
      
      const filteredTree = filterDeletedComments(tree);
      setComments(filteredTree);
    } catch (err) {
      console.error(err);
    } finally {
      setCommentsLoading(false);
    }
  }, [recipe?.id, accessToken]);

  useEffect(() => {
    if (recipe) {
      fetchComments();
    }
  }, [recipe, fetchComments]);

  const getErrorMessage = async (response) => {
    try {
      const data = await response.json();
      return data.error || data.message || 'Неизвестная ошибка';
    } catch {
      try {
        return await response.text();
      } catch {
        return `Ошибка ${response.status}`;
      }
    }
  };

  const handleSubmitComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim()) return;
    setCommentError(null);
    setSubmitting(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${recipe.id}/comments`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ content: newComment.trim() }),
      });
      if (!response.ok) {
        const errorMsg = await getErrorMessage(response);
        throw new Error(errorMsg);
      }
      setNewComment('');
      fetchComments();
    } catch (err) {
      setCommentError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleReplySubmit = async (parentId) => {
    if (!replyContent.trim()) return;
    setCommentError(null);
    setSubmitting(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${recipe.id}/comments/${parentId}/reply`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ content: replyContent.trim() }),
      });
      if (!response.ok) {
        const errorMsg = await getErrorMessage(response);
        throw new Error(errorMsg);
      }
      setReplyContent('');
      setReplyingToId(null);
      fetchComments();
    } catch (err) {
      setCommentError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleEditSubmit = async (commentId) => {
    if (!editingContent.trim()) return;
    setSubmitting(true);
    setCommentError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${recipe.id}/comments/${commentId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ content: editingContent.trim() }),
      });
      if (!response.ok) {
        const errorMsg = await getErrorMessage(response);
        throw new Error(errorMsg);
      }
      setEditingCommentId(null);
      setEditingContent('');
      fetchComments();
    } catch (err) {
      setCommentError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteComment = async () => {
    if (!deletingCommentId) return;
    setCommentError(null);
    setSubmitting(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${recipe.id}/comments/${deletingCommentId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) {
        const errorMsg = await getErrorMessage(response);
        throw new Error(errorMsg);
      }
      setDeletingCommentId(null);
      fetchComments();
    } catch (err) {
      setCommentError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleComplaintSubmit = async () => {
    if (!complaintCommentId) return;
    setSubmitting(true);
    setCommentError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${recipe.id}/comments/${complaintCommentId}/complaints`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ reason: complaintReason.trim() || null }),
      });
      if (!response.ok) {
        const errorMsg = await getErrorMessage(response);
        throw new Error(errorMsg);
      }
      setComplaintDialogOpen(false);
      setComplaintReason('');
      setComplaintCommentId(null);
    } catch (err) {
      setCommentError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const filterLinks = (text) => {
    const urlRegex = /(https?:\/\/[^\s]+)/g;
    return text.replace(urlRegex, '[ссылка удалена]');
  };

  const handleApproveRecipe = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/admin/recipes/moderate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ recipeId: parseInt(id), isApproved: true }),
      });
      if (response.ok) {
        await fetchRecipe();
      } else {
        const errorData = await response.json();
        alert(errorData.message || 'Ошибка при принятии рецепта');
      }
    } catch (error) {
      console.error('Ошибка при принятии рецепта:', error);
    }
  };

  const handleRejectRecipe = async () => {
    if (!rejectionComment.trim()) {
      alert('Введите причину отклонения');
      return;
    }
    try {
      const response = await fetch(`${API_BASE_URL}/api/admin/recipes/moderate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({
          recipeId: parseInt(id),
          isApproved: false,
          rejectionComment: rejectionComment.trim(),
        }),
      });
      if (response.ok) {
        setRejectModalOpen(false);
        setRejectionComment('');
        await fetchRecipe();
      } else {
        const errorData = await response.json();
        alert(errorData.message || 'Ошибка при отклонении рецепта');
      }
    } catch (error) {
      console.error('Ошибка при отклонении рецепта:', error);
    }
  };

  const handleExportPDF = async () => {
    if (!recipe) return;
    setExporting(true);
    try {
      const blob = await pdf(<RecipePDF recipe={recipe} />).toBlob();
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${recipe.name}.pdf`;
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

  const promptDelete = () => setDeleteModalOpen(true);

  const confirmDelete = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/recipes/${id}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (response.status === 401) {
        navigate('/login');
        return;
      }
      if (!response.ok) throw new Error('Ошибка при удалении рецепта');
      navigate('/recipes');
    } catch (err) {
      setError(err.message);
    } finally {
      setDeleteModalOpen(false);
    }
  };

  const formatIngredient = (ingredient) => {
    if (!ingredient.ingredientName) return 'Не указано';
    let displayText = ingredient.ingredientName;
    if (ingredient.unitName !== null && ingredient.quantity !== undefined) {
      if (ingredient.quantity) {
        displayText = `${ingredient.ingredientName}: ${ingredient.quantity} ${ingredient.unitName}`;
      } else {
        displayText = `${ingredient.ingredientName}: ${ingredient.unitName}`;
      }
    }
    return displayText;
  };

  const formatEquipment = (equipments) => {
    if (!equipments?.length) return 'Не указано';
    const names = equipments.map((item) => item.equipmentName.toLowerCase());
    names[0] = names[0].charAt(0).toUpperCase() + names[0].slice(1);
    return names.join(', ');
  };

  const NutritionCards = ({ recipe, type }) => {
    let calories, proteins, fats, carbohydrates;
    switch (type) {
      case 'total':
        calories = recipe.totalCalories;
        proteins = recipe.totalProteins;
        fats = recipe.totalFats;
        carbohydrates = recipe.totalCarbohydrates;
        break;
      case 'serving':
        calories = recipe.caloriesPerServing;
        proteins = recipe.proteinsPerServing;
        fats = recipe.fatsPerServing;
        carbohydrates = recipe.carbohydratesPerServing;
        break;
      case 'per100g':
        calories = recipe.caloriesPer100g;
        proteins = recipe.proteinsPer100g;
        fats = recipe.fatsPer100g;
        carbohydrates = recipe.carbohydratesPer100g;
        break;
      default:
        return null;
    }
    return (
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div className="bg-purple-50 p-4 rounded-xl text-center">
          <div className="text-xl font-bold text-purple-700">{calories.toFixed(1)}</div>
          <div className="text-sm text-gray-600">ккал</div>
        </div>
        <div className="bg-blue-50 p-4 rounded-xl text-center">
          <div className="text-xl font-bold text-blue-700">{proteins.toFixed(1)}</div>
          <div className="text-sm text-gray-600">белки</div>
        </div>
        <div className="bg-yellow-50 p-4 rounded-xl text-center">
          <div className="text-xl font-bold text-yellow-700">{fats.toFixed(1)}</div>
          <div className="text-sm text-gray-600">жиры</div>
        </div>
        <div className="bg-green-50 p-4 rounded-xl text-center">
          <div className="text-xl font-bold text-green-700">{carbohydrates.toFixed(1)}</div>
          <div className="text-sm text-gray-600">углеводы</div>
        </div>
      </div>
    );
  };

  const buildCommentTree = (flatComments) => {
    const map = {};
    const roots = [];

    flatComments.forEach(comment => {
      map[comment.id] = { ...comment, replies: [] };
    });

    flatComments.forEach(comment => {
      if (comment.parentCommentId && map[comment.parentCommentId]) {
        map[comment.parentCommentId].replies.push(map[comment.id]);
      } else {
        roots.push(map[comment.id]);
      }
    });

    return roots;
  };

const renderComment = (comment, level = 0) => {
  const isAuthor = user?.userId && String(user.userId) === String(comment.userId);
  const isAdmin = user?.roles?.includes('Admin') || user?.roles?.includes('Moderator');
  const canModify = (isAuthor || isAdmin) && !comment.isDeleted; // запрещаем действия над удалённым комментарием
  const isEditing = editingCommentId === comment.id;
  const isReplying = replyingToId === comment.id;

  return (
    <div key={comment.id} className="border-b border-gray-100 pb-4 last:border-0" style={{ marginLeft: level * 20 }}>
      <div className="flex items-start gap-3">
        <Link to={`/users/${comment.userId}`}>
          <Avatar className="h-8 w-8">
            {comment.userAvatarUrl ? (
              <AvatarImage src={comment.userAvatarUrl} />
            ) : (
              <AvatarFallback className="bg-gradient-to-br from-[#201469] to-[#4c1d95] text-white">
                {comment.userName?.charAt(0).toUpperCase() || 'U'}
              </AvatarFallback>
            )}
          </Avatar>
        </Link>
        <div className="flex-1">
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center gap-2 flex-wrap">
              <UserAchievementsTooltip userId={comment.userId}>
                <Link to={`/users/${comment.userId}`} className="font-medium hover:text-primary transition-colors">
                  {comment.userName || 'Неизвестный'}
                </Link>
              </UserAchievementsTooltip>
              <span className="text-muted-foreground">·</span>
              <span className="text-muted-foreground">
                {new Date(comment.createdAt).toLocaleDateString('ru-RU')}
              </span>
              {comment.updatedAt && !comment.isDeleted && (
                <>
                  <span className="text-muted-foreground">·</span>
                  <span className="text-muted-foreground text-xs">(ред.)</span>
                </>
              )}
            </div>

            {user && !comment.isDeleted && (
              <div className="flex items-center gap-1">
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => {
                    setReplyingToId(comment.id);
                    setReplyContent('');
                  }}
                  title="Ответить"
                >
                  <Reply className="h-4 w-4" />
                </Button>
                {canModify && (
                  <>
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => {
                        setEditingCommentId(comment.id);
                        setEditingContent(comment.content);
                      }}
                      title="Редактировать"
                    >
                      <Edit2 className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="text-destructive hover:text-destructive"
                      onClick={() => setDeletingCommentId(comment.id)}
                      title="Удалить"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </>
                )}
                {user.userId !== comment.userId && (
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => {
                      setComplaintCommentId(comment.id);
                      setComplaintReason('');
                      setComplaintDialogOpen(true);
                    }}
                    title="Пожаловаться"
                  >
                    <Flag className="h-4 w-4" />
                  </Button>
                )}
              </div>
            )}
          </div>

          {isEditing ? (
            <div className="mt-2">
              <Textarea
                value={editingContent}
                onChange={(e) => {
                  setEditingContent(e.target.value);
                  setCommentError(null);
                }}
                className="mb-2"
                autoFocus
              />
              <div className="flex gap-2">
                <Button size="sm" onClick={() => handleEditSubmit(comment.id)} disabled={submitting}>
                  <Check className="h-4 w-4 mr-1" /> Сохранить
                </Button>
                <Button size="sm" variant="ghost" onClick={() => setEditingCommentId(null)}>
                  <X className="h-4 w-4 mr-1" /> Отмена
                </Button>
              </div>
            </div>
          ) : (
            <>
              {comment.isDeleted ? (
                <p className="text-gray-500 italic mt-1">Комментарий удалён</p>
              ) : (
                <p className="text-gray-700 mt-1 break-words break-all whitespace-pre-wrap">
                  {filterLinks(comment.content)}
                </p>
              )}
            </>
          )}

          {isReplying && (
            <div className="mt-4 ml-8">
              <div className="flex items-start gap-3">
                <Link to={`/users/${user?.userId}`}>
                  <Avatar className="h-6 w-6">
                    {userAvatar ? (
                      <AvatarImage src={userAvatar} />
                    ) : (
                      <AvatarFallback className="bg-gradient-to-br from-[#201469] to-[#4c1d95] text-white text-xs">
                        {user?.username?.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    )}
                  </Avatar>
                </Link>
                <div className="flex-1">
                  <Textarea
                    placeholder={`Ответить ${comment.userName}...`}
                    value={replyContent}
                    onChange={(e) => {
                      setReplyContent(e.target.value);
                      setCommentError(null);
                    }}
                    className="mb-2"
                  />
                  <div className="flex gap-2">
                    <Button size="sm" onClick={() => handleReplySubmit(comment.id)} disabled={submitting}>
                      Отправить
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setReplyingToId(null)}>
                      Отмена
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
      {comment.replies && comment.replies.length > 0 && (
        <div className="mt-2">
          {comment.replies.map((reply) => renderComment(reply, level + 1))}
        </div>
      )}
    </div>
  );
};

  const isAdmin = user?.roles?.includes('Admin') || user?.roles?.includes('Moderator');
  const isCreator = recipe?.creatorId === user?.userId || recipe?.creatorId === user?.id;
  const showActions = isAdmin || isCreator;
  const canModerate = isAdmin && recipe?.isApproved === null;

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary"></div>
          <p className="mt-4 text-gray-600">Загрузка рецепта...</p>
        </div>
      </div>
    );
  }

  if (!recipe) {
    return <div className="text-center py-12">Рецепт не найден</div>;
  }

  return (
    <div className="container mx-auto p-4 max-w-5xl relative">
      {error && (
        <div className="mb-6 p-4 bg-destructive/10 text-destructive rounded-lg border border-destructive/20">
          {error}
        </div>
      )}
      <InterestingFactWidget recipeId={recipe.id} />
      <div className="mb-6 space-y-2">
        {recipe.isApproved === null && (
          <div className="flex items-center justify-between p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <div className="flex items-center gap-3">
              <Badge variant="outline" className="bg-yellow-100 text-yellow-800 border-yellow-300">
                На проверке
              </Badge>
              <span className="text-yellow-800">Ожидает модерации</span>
            </div>
            {canModerate && (
              <div className="flex gap-2">
                <Button size="sm" onClick={handleApproveRecipe} className="gap-1">
                  <FaCheck className="h-4 w-4" /> Принять
                </Button>
                <Button size="sm" variant="destructive" onClick={() => setRejectModalOpen(true)} className="gap-1">
                  <FaTimes className="h-4 w-4" /> Отклонить
                </Button>
              </div>
            )}
          </div>
        )}
        {recipe.isApproved === false && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-lg">
            <div className="flex items-center gap-2 mb-2">
              <Badge variant="destructive">Отклонён</Badge>
              <span className="text-red-800 font-medium">Причина: {recipe.rejectionComment}</span>
            </div>
          </div>
        )}
        {recipe.isApproved === true && (isAdmin || user?.roles?.includes('Moderator')) && (
          <div className="p-4 bg-green-50 border border-green-200 rounded-lg flex items-center gap-3">
            <Badge variant="outline" className="bg-green-100 text-green-800 border-green-300">
              Утверждён
            </Badge>
          </div>
        )}
      </div>

      <div className="relative mb-8">
        {recipe.imageUrl && (
          <img
            src={recipe.originalImageUrl || recipe.imageUrl}
            alt={recipe.name}
            className="w-full h-80 md:h-96 object-cover rounded-xl"
          />
        )}
        <Button
          variant="default"
          size="icon"
          className="absolute top-4 right-30 z-10 bg-white/80 backdrop-blur-sm hover:bg-white rounded-full"
          onClick={handleToggleShoppingList}
          title={isInShoppingList ? "Убрать из списка покупок" : "Добавить в список покупок"}
        >
          <ShoppingCart className={`h-5 w-5 ${isInShoppingList ? 'text-violet-600 fill-violet-600' : 'text-gray-600'}`} />
        </Button>

        <Button
          variant="default"
          size="icon"
          className="absolute top-4 right-17 z-10 bg-white/80 backdrop-blur-sm hover:bg-white rounded-full"
          onClick={handleExportPDF}
          disabled={exporting}
          title="Скачать рецепт в PDF"
        >
          {exporting ? <Loader2 className="h-5 w-5 animate-spin" /> : <FileDown className="h-5 w-5 text-gray-600" />}
        </Button>

        <Button
          variant="default"
          size="icon"
          className="absolute top-4 right-4 z-10 bg-white/80 backdrop-blur-sm hover:bg-white rounded-full"
          onClick={handleToggleFavorite}
        >
          {isFavorite ? <FaHeart className="text-red-500 h-5 w-5" /> : <FaRegHeart className="h-5 w-5" />}
        </Button>
        <div className="absolute top-4 left-4 z-10 bg-white/80 backdrop-blur-sm px-3 py-1.5 rounded-full flex items-center gap-1">
          <FaStar className="text-yellow-500 h-4 w-4" />
          <span className="font-medium">{ratingSummary.averageRating.toFixed(1)}</span>
          <span className="text-sm text-gray-500">({ratingSummary.totalRatings})</span>
        </div>
        <div className="absolute bottom-4 left-4 right-4 bg-gradient-to-t from-black/70 to-transparent p-4 rounded-b-xl">
          <h1 className="text-3xl font-bold text-white mb-1">{recipe.name}</h1>
        </div>
      </div>

      <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
        <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-xl">
          <div>
            <p className="text-sm text-gray-500">Время</p>
            <p className="font-medium">{recipe.cookingTimeWithoutUser} / {recipe.cookingTimeWithUser} мин</p>
          </div>
        </div>
        <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-xl">
          <div>
            <p className="text-sm text-gray-500 mb-2">Сложность</p>
            <VisualRating value={recipe.difficultyLevel} max={5} FilledIcon={GiWhisk} OutlinedIcon={GiWhisk} filledClass="text-green-500" outlinedClass="text-gray-300" />
          </div>
        </div>
        <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-xl">
          <div>
            <p className="text-sm text-gray-500 mb-2">Острота</p>
            <VisualRating value={recipe.spicinessLevel} max={5} FilledIcon={FaPepperHot} OutlinedIcon={FaPepperHot} filledClass="text-red-500" outlinedClass="text-gray-300" />
          </div>
        </div>
        <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-xl">
          <div>
            <p className="text-sm text-gray-500">Тип блюда</p>
            <p className="font-medium">{recipe.dishType?.name || '—'}</p>
          </div>
        </div>
        <div className="flex items-center gap-3 p-4 bg-gray-50 rounded-xl">
          <div>
            <p className="text-sm text-gray-500">Порций</p>
            <p className="font-medium">{recipe.servings}</p>
          </div>
        </div>
      </div>

      <div className="grid md:grid-cols-2 gap-6 mb-8">
        <Card className="border-0 shadow-none">
          <CardContent className="p-0">
            <h2 className="text-xl font-semibold mb-3">Ингредиенты</h2>
            <ul className="space-y-1 text-gray-700">
              {recipe.recipeIngredients?.length > 0 ? (
                recipe.recipeIngredients.map((ing, idx) => <li key={idx}>{formatIngredient(ing)}</li>)
              ) : (
                <li>Не указано</li>
              )}
            </ul>
          </CardContent>
        </Card>
        <Card className="border-0 shadow-none">
          <CardContent className="p-0">
            <h2 className="text-xl font-semibold mb-3">Оборудование</h2>
            <p className="text-gray-700">{formatEquipment(recipe.recipeEquipments)}</p>
          </CardContent>
        </Card>
      </div>

      <Card className="border-0 shadow-none mb-8">
        <CardContent className="p-0">
          <Collapsible open={isOpen} onOpenChange={setIsOpen}>
            <div className="flex items-center justify-between">
              <h2 className="text-xl font-semibold">Приготовление</h2>
              <CollapsibleTrigger asChild>
                <Button variant="ghost" size="sm" className="gap-1">
                  {isOpen ? 'Свернуть' : 'Показать'}
                  <ChevronDown className={`h-4 w-4 transition-transform ${isOpen ? 'rotate-180' : ''}`} />
                </Button>
              </CollapsibleTrigger>
            </div>
            
            <CollapsibleContent className="mt-4">
              <div className="border-t border-gray-200 pt-6">
                <div className="prose-content" dangerouslySetInnerHTML={{ __html: recipe.fullDescription }} />
              </div>
              <div className="border-t border-gray-200 mt-6"></div>
            </CollapsibleContent>
          </Collapsible>
        </CardContent>
      </Card>

      <Card className="border-0 shadow-none mb-8">
        <CardContent className="p-0">
          <h2 className="text-xl font-semibold mb-4">Пищевая ценность</h2>
          <Tabs defaultValue="serving" onValueChange={setActiveNutritionTab}>
            <TabsList className="grid w-full grid-cols-3 bg-transparent h-auto p-0 border-b border-gray-200">
              <TabsTrigger
                value="total"
                className="data-[state=active]:border-b-2 data-[state=active]:border-m-violet data-[state=active]:text-primary rounded-none bg-transparent py-2 px-4 text-gray-600 hover:text-primary"
              >
                На всё блюдо
              </TabsTrigger>
              <TabsTrigger
                value="serving"
                disabled={!recipe.servings}
                className="data-[state=active]:border-b-2 data-[state=active]:border-m-violet data-[state=active]:text-primary rounded-none bg-transparent py-2 px-4 text-gray-600 hover:text-primary disabled:opacity-50"
              >
                На порцию
              </TabsTrigger>
              <TabsTrigger
                value="per100g"
                className="data-[state=active]:border-b-2 data-[state=active]:border-m-violet data-[state=active]:text-primary rounded-none bg-transparent py-2 px-4 text-gray-600 hover:text-primary"
              >
                На 100 г
              </TabsTrigger>
            </TabsList>
            <TabsContent value="total" className="mt-4">
              <NutritionCards recipe={recipe} type="total" />
            </TabsContent>
            <TabsContent value="serving" className="mt-4">
              <NutritionCards recipe={recipe} type="serving" />
            </TabsContent>
            <TabsContent value="per100g" className="mt-4">
              <NutritionCards recipe={recipe} type="per100g" />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      <Card className="border-0 shadow-none mb-8">
        <CardContent className="p-0">
          <h2 className="text-xl font-semibold mb-4">Рейтинг</h2>
          <div className="flex flex-col md:flex-row items-center justify-between gap-6">
            <div className="text-center">
              <div className="text-4xl font-bold text-yellow-600">{ratingSummary.averageRating.toFixed(1)}</div>
              <VisualRating
                value={Math.round(ratingSummary.averageRating)}
                max={5}
                FilledIcon={FaStar}
                OutlinedIcon={FaRegStar}
                filledClass="text-yellow-500"
                containerClass="justify-center my-2"
              />
              <div className="text-sm text-gray-500">{ratingSummary.totalRatings} оценок</div>
            </div>
            <div className="flex-1 max-w-md">
              {[5, 4, 3, 2, 1].map((star) => (
                <div key={star} className="flex items-center gap-2 text-sm">
                  <span className="w-4">{star}</span>
                  <FaStar className="text-yellow-500 h-3 w-3" />
                  <div className="flex-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-yellow-500"
                      style={{
                        width: `${((ratingSummary.ratingDistribution[star] || 0) / (ratingSummary.totalRatings || 1)) * 100}%`,
                      }}
                    />
                  </div>
                  <span className="w-8 text-right text-gray-600">{ratingSummary.ratingDistribution[star] || 0}</span>
                </div>
              ))}
            </div>
            <div className="text-center">
              <h3 className="font-medium mb-2">Ваша оценка</h3>
              {canRate ? (
                <div>
                  <VisualRatingSelect
                    value={ratingSummary.userRating || 0}
                    onChange={handleRateRecipe}
                    max={5}
                    FilledIcon={FaStar}
                    OutlinedIcon={FaRegStar}
                    filledClass="text-yellow-500"
                    containerClass="justify-center"
                  />
                  {ratingSummary.userRating && (
                    <Button variant="link" size="sm" onClick={handleRemoveRating} className="mt-2">
                      Удалить
                    </Button>
                  )}
                </div>
              ) : (
                <span className="text-sm text-gray-500">Вы автор</span>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      <Card className="border-0 shadow-none mb-8">
        <CardContent className="p-0">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-2xl font-semibold">Комментарии</h3>
            <Button
              variant="ghost"
              size="sm"
              className="gap-1"
              onClick={() => setShowComments(!showComments)}
            >
              {showComments ? 'Скрыть' : 'Показать'}
              <ChevronDown className={`h-4 w-4 transition-transform ${showComments ? 'rotate-180' : ''}`} />
            </Button>
          </div>

          {showComments && (
            <>
              {commentError && (
                <div className="mb-4 p-3 bg-destructive/10 text-destructive rounded-lg border border-destructive/20 text-sm">
                  {commentError}
                </div>
              )}
              {user ? (
                <form onSubmit={handleSubmitComment} className="mb-6">
                  <Textarea
                    placeholder="Напишите комментарий..."
                    value={newComment}
                    onChange={(e) => {
                      setNewComment(e.target.value);
                      setCommentError(null);
                    }}
                    className="mb-2"
                  />
                  <Button type="submit" variant="violet" disabled={submitting || !newComment.trim()}>
                    {submitting ? 'Отправка...' : 'Отправить'}
                  </Button>
                </form>
              ) : (
                <p className="text-muted-foreground mb-6">
                  Чтобы оставить комментарий, <Link to="/login" className="text-primary hover:underline">войдите</Link>.
                </p>
              )}

              {commentsLoading && comments.length === 0 ? (
                <div className="flex justify-center py-4">
                  <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </div>
              ) : (
                <div className="space-y-4">
                  {comments.map((comment) => renderComment(comment))}
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      {recipe.creatorUsername && (
        <div className="flex items-center justify-center gap-3 py-4 border-t border-gray-100">
          <Avatar>
            {recipe.creatorAvatarUrl ? (
              <AvatarImage src={recipe.creatorAvatarUrl} onError={(e) => e.target.style.display = 'none'} />
            ) : null}
            <AvatarFallback className="bg-gradient-to-br from-[#201469] to-[#4c1d95] text-white text-2xl">
              {recipe.creatorUsername.charAt(0).toUpperCase() || 'U'}
            </AvatarFallback>
          </Avatar>
          <div>
            <p className="text-sm text-gray-500">Автор</p>
            <Link to={`/users/${recipe.creatorId}`} className="font-medium hover:text-primary transition-colors">
              {recipe.creatorUsername}
            </Link>
          </div>
        </div>
      )}
      
      {recipe && (
        <div className="flex justify-center gap-4 mt-8">
          {showActions && (
            <>
              <Button variant="violet" onClick={() => navigate(`/recipes/edit/${recipe.id}`)}>
                Редактировать
              </Button>
              <Button variant="dashedHover" onClick={promptDelete}>
                Удалить
              </Button>
            </>
          )}
        </div>
      )}

      <AlertDialog open={complaintDialogOpen} onOpenChange={setComplaintDialogOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Пожаловаться на комментарий</AlertDialogTitle>
            <AlertDialogDescription>
              Опишите причину жалобы (необязательно). Администратор рассмотрит ваше обращение.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <Textarea
            placeholder="Причина (необязательно)"
            value={complaintReason}
            onChange={(e) => setComplaintReason(e.target.value)}
            className="my-4"
          />
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setComplaintDialogOpen(false)}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={handleComplaintSubmit} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              {submitting ? 'Отправка...' : 'Отправить жалобу'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={!!deletingCommentId} onOpenChange={() => setDeletingCommentId(null)}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Удалить комментарий?</AlertDialogTitle>
            <AlertDialogDescription>
              Это действие нельзя отменить. Комментарий будет удалён навсегда.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setDeletingCommentId(null)}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={handleDeleteComment} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Удалить
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={rejectModalOpen} onOpenChange={setRejectModalOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Отклонить рецепт</AlertDialogTitle>
            <AlertDialogDescription>
              Укажите причину отклонения рецепта "{recipe?.name}":
            </AlertDialogDescription>
          </AlertDialogHeader>
          <textarea
            value={rejectionComment}
            onChange={(e) => setRejectionComment(e.target.value)}
            className="w-full px-3 py-2 border rounded-md"
            rows={4}
            placeholder="Введите причину..."
          />
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setRejectionComment('')}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={handleRejectRecipe} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Отклонить
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={deleteModalOpen} onOpenChange={setDeleteModalOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Подтверждение удаления</AlertDialogTitle>
            <AlertDialogDescription>
              Вы уверены, что хотите удалить этот рецепт? Это действие нельзя отменить.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={confirmDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              Удалить
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default RecipeDetailPage;