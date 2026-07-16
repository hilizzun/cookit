import { useLocation, useNavigate } from "react-router-dom";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Card, CardContent } from "../components/ui/card";

export default function EmailVerificationSentPage() {
  const { state } = useLocation();
  const email = state?.email || "";
  const navigate = useNavigate();

  return (
    <div className="flex items-center justify-center">
      <Card className="w-full max-w-md">
        <CardContent className="p-8 space-y-6">
          <div className="flex justify-center">
            <Link to="/">
              <img src="/logo.svg" alt="CookIt" className="h-15" />
            </Link>
          </div>
          <div className="text-center space-y-4">
            <h2 className="text-2xl font-bold">Проверьте почту!</h2>
            <p className="text-muted-foreground">
              Письмо с подтверждением отправлено на<br />
              <span className="font-medium">{email}</span>
            </p>
            <p>Перейдите по ссылке в письме, чтобы активировать аккаунт.</p>
            <div className="space-y-2">
              <Button variant="violet" className="w-full" onClick={() => navigate("/")}>
                На главную
              </Button>
              <Button variant="outline" className="w-full" onClick={() => navigate(`/resend-verification?email=${email}`)}>
                Отправить повторно
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}