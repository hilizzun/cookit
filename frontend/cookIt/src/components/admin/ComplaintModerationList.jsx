import React, { useState, useEffect } from 'react';
import { useAuth } from "../../contexts/AuthContext";
import { useNavigate } from "react-router-dom";
import { API_BASE_URL } from '../../config/settings';
import { Button } from '../ui/button';
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
import { FaEye, FaTrash, FaTimes, FaCheck, FaUser } from "react-icons/fa";
import { Loader2 } from 'lucide-react';

const ComplaintModerationList = () => {
  const { accessToken } = useAuth();
  const navigate = useNavigate();
  const [complaints, setComplaints] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedComplaint, setSelectedComplaint] = useState(null);
  const [resolveDialog, setResolveDialog] = useState({ open: false, type: null }); 
  const [resolutionNote, setResolutionNote] = useState("");

  const fetchComplaints = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${API_BASE_URL}/api/admin/complaints/pending`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка загрузки жалоб');
      const data = await response.json();
      setComplaints(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchComplaints();
  }, [accessToken]);

  const handleResolve = async (complaintId, deleteComment) => {
    try {
        const response = await fetch(`${API_BASE_URL}/api/admin/complaints/${complaintId}/resolve`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify({
            deleteComment,
            resolutionNote: resolutionNote.trim() || undefined,
        }),
        });
      if (!response.ok) throw new Error('Ошибка обработки жалобы');
      setResolveDialog({ open: false, type: null });
      setResolutionNote("");
      fetchComplaints();
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
        <h3 className="text-lg font-semibold">Жалобы на комментарии</h3>
        <Button variant="outline" size="sm" onClick={fetchComplaints}>
          Обновить
        </Button>
      </div>

      {error && (
        <div className="p-4 bg-destructive/10 text-destructive rounded-lg">
          {error}
        </div>
      )}

      {complaints.length === 0 ? (
        <div className="text-center py-12 bg-muted rounded-lg">
          <FaCheck className="mx-auto text-4xl text-green-500 mb-4" />
          <h3 className="text-xl font-semibold">Нет новых жалоб</h3>
          <p className="text-muted-foreground mt-2">Все жалобы обработаны.</p>
        </div>
      ) : (
        <div className="border rounded-lg overflow-hidden">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Комментарий</TableHead>
                <TableHead>Автор жалобы</TableHead>
                <TableHead>Причина</TableHead>
                <TableHead>Дата</TableHead>
                <TableHead className="text-right">Действия</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {complaints.map((complaint) => (
                <TableRow key={complaint.id}>
                  <TableCell>
                    <div className="max-w-xs">
                      <p className="font-medium">{complaint.commentContent}</p>
                      <Button
                        variant="link"
                        size="sm"
                        className="p-0 h-auto text-xs"
                        onClick={() => navigate(`/recipes/${complaint.recipeId}`)}
                      >
                        Перейти к рецепту
                      </Button>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => navigate(`/users/${complaint.userId}`)}
                        title="Перейти к автору жалобы"
                      >
                      <div className="flex items-center gap-2">
                        {complaint.userName}
                        <FaUser className="h-3 w-3" />
                      </div>
                    </Button>
                    
                  </TableCell>
                  <TableCell>{complaint.reason || '—'}</TableCell>
                  <TableCell>{new Date(complaint.createdAt).toLocaleDateString('ru-RU')}</TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-green-600 hover:text-green-700"
                        onClick={() => {
                          setSelectedComplaint(complaint);
                          setResolveDialog({ open: true, type: 'delete' });
                        }}
                      >
                        Удалить комментарий
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-destructive hover:text-destructive"
                        onClick={() => {
                          setSelectedComplaint(complaint);
                          setResolveDialog({ open: true, type: 'reject' });
                        }}
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

      <AlertDialog
        open={resolveDialog.open}
        onOpenChange={(open) => !open && setResolveDialog({ open: false, type: null })}
      >
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>
              {resolveDialog.type === 'delete' ? 'Удалить комментарий' : 'Отклонить жалобу'}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {resolveDialog.type === 'delete'
                ? 'Вы уверены, что хотите удалить этот комментарий? Жалоба будет помечена как обработанная.'
                : 'Вы уверены, что хотите отклонить жалобу? Комментарий останется.'}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <textarea
            value={resolutionNote}
            onChange={(e) => setResolutionNote(e.target.value)}
            className="w-full px-3 py-2 border rounded-md"
            rows={3}
            placeholder="Примечание (необязательно)"
          />
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setResolutionNote("")}>Отмена</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => handleResolve(selectedComplaint.id, resolveDialog.type === 'delete')}
              className={resolveDialog.type === 'delete' ? 'bg-destructive text-destructive-foreground hover:bg-destructive/90' : ''}
            >
              Подтвердить
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default ComplaintModerationList;