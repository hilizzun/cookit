import React, { useState, useEffect } from 'react';
import { useAuth } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import { API_BASE_URL } from '../../config/settings';
import { Button } from '../ui/button';
import { Badge } from '../ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '../ui/alert-dialog';
import { FaCheck, FaTimes, FaEye, FaClock } from "react-icons/fa";
import { Loader2 } from 'lucide-react';

const RecipeModerationList = () => {
  const { accessToken } = useAuth();
  const navigate = useNavigate();
  const [recipes, setRecipes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [rejectDialog, setRejectDialog] = useState({ open: false, recipeId: null });
  const [rejectionComment, setRejectionComment] = useState("");

  const fetchRecipesForModeration = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${API_BASE_URL}/api/admin/recipes/moderation`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка загрузки рецептов');
      const data = await response.json();
      setRecipes(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchRecipesForModeration();
  }, [accessToken]);

  const moderateRecipe = async (recipeId, isApproved, comment = "") => {
    try {
      const response = await fetch(`${API_BASE_URL}/api/admin/recipes/moderate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({
          recipeId,
          isApproved,
          rejectionComment: comment,
        }),
      });
      if (!response.ok) throw new Error('Ошибка модерации');
      setRejectDialog({ open: false, recipeId: null });
      setRejectionComment("");
      await fetchRecipesForModeration();
    } catch (err) {
      setError(err.message);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold">Рецепты на модерацию</h3>
        <Button variant="outline" size="sm" onClick={fetchRecipesForModeration}>
          Обновить
        </Button>
      </div>

      {error && (
        <div className="p-4 bg-destructive/10 text-destructive rounded-lg">
          {error}
        </div>
      )}

      {recipes.length === 0 ? (
        <div className="text-center py-12 bg-muted rounded-lg">
          <FaCheck className="mx-auto text-4xl text-green-500 mb-4" />
          <h3 className="text-xl font-semibold">Нет рецептов для модерации</h3>
          <p className="text-muted-foreground mt-2">Все рецепты проверены!</p>
        </div>
      ) : (
        <div className="border rounded-lg overflow-hidden">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Название</TableHead>
                <TableHead>Статус</TableHead>
                <TableHead className="text-right">Действия</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {recipes.map((recipe) => (
                <TableRow key={recipe.id}>
                  <TableCell>
                    <div className="font-medium">{recipe.name}</div>
                    <div className="text-sm text-muted-foreground">{recipe.shortDescription}</div>
                  </TableCell>
                  <TableCell>
                    <Badge variant="outline" className="bg-yellow-100 text-yellow-800 border-yellow-300">
                      <FaClock className="inline mr-1 h-3 w-3" />
                      На проверке
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => navigate(`/recipes/${recipe.id}`)}
                      >
                        <FaEye className="h-4 w-4 mr-1" />
                        Посмотреть
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-green-600 hover:text-green-700"
                        onClick={() => moderateRecipe(recipe.id, true)}
                      >
                        <FaCheck className="h-4 w-4 mr-1" />
                        Принять
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        onClick={() => setRejectDialog({ open: true, recipeId: recipe.id })}
                      >
                        <FaTimes className="h-4 w-4 mr-1" />
                        Отклонить
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <AlertDialog open={rejectDialog.open} onOpenChange={(open) => setRejectDialog({ open, recipeId: null })}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Отклонение рецепта</AlertDialogTitle>
            <AlertDialogDescription>
              Укажите причину отклонения рецепта:
            </AlertDialogDescription>
          </AlertDialogHeader>
          <textarea
            value={rejectionComment}
            onChange={(e) => setRejectionComment(e.target.value)}
            className="w-full px-3 py-2 border rounded-md"
            rows={4}
            placeholder="Причина отклонения..."
          />
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setRejectionComment("")}>Отмена</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => moderateRecipe(rejectDialog.recipeId, false, rejectionComment)}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Отклонить
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default RecipeModerationList;