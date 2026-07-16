import React, { useEffect, useState } from "react";
import { useParams, useNavigate, Link } from "react-router-dom";
import { confirmEmail } from "../services/authService";
import { Button } from "../components/ui/button";
import { Card, CardContent } from "../components/ui/card";
import { Loader2 } from "lucide-react";

export default function ConfirmEmailPage() {
  const { userId, token: encodedToken } = useParams();
  const [status, setStatus] = useState("loading");
  const [errorMessage, setErrorMessage] = useState("");
  const navigate = useNavigate();

  useEffect(() => {
    const confirm = async () => {
      try {
        const response = await confirmEmail(userId, encodedToken);
        if (response.redirectPath) {
          navigate(response.redirectPath);
        } else if (response.message) {
          setStatus("success");
          setTimeout(() => navigate("/login"), 3000);
        }
      } catch (err) {
        console.error("Confirmation error:", err);
        setStatus("error");
        setErrorMessage(err.message || "Ошибка подтверждения");
      }
    };
    confirm();
  }, [userId, encodedToken, navigate]);

  return (
    <div className="flex items-center justify-center min-h-screen p-4">
      <Card className="w-full max-w-md">
        <CardContent className="p-8">
          {status === "loading" && (
            <div className="text-center space-y-4">
              <Loader2 className="h-12 w-12 animate-spin mx-auto text-primary" />
              <h2 className="text-xl font-semibold">Подтверждение email</h2>
              <p className="text-muted-foreground">Пожалуйста, подождите...</p>
            </div>
          )}
          {status === "success" && (
            <div className="text-center space-y-4">
              <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto">
                <svg className="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
              </div>
              <h2 className="text-2xl font-bold">Email подтверждён!</h2>
              <p>Сейчас вы будете перенаправлены на страницу входа.</p>
              <div className="h-1 w-full bg-secondary rounded-full overflow-hidden">
                <div className="h-full bg-primary animate-progress"></div>
              </div>
              <p className="text-sm text-muted-foreground">
                Или <Link to="/login" className="text-primary hover:underline">перейти сейчас</Link>
              </p>
            </div>
          )}
          {status === "error" && (
            <div className="text-center space-y-4">
              <div className="w-16 h-16 bg-destructive/10 rounded-full flex items-center justify-center mx-auto">
                <svg className="w-8 h-8 text-destructive" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </div>
              <h2 className="text-2xl font-bold text-destructive">Ошибка</h2>
              <p className="text-destructive">{errorMessage}</p>
              <Button variant="violet" className="w-full" asChild>
                <Link to="/resend-verification">Отправить письмо повторно</Link>
              </Button>
              <Button variant="ghost" className="w-full" asChild>
                <Link to="/">На главную</Link>
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}