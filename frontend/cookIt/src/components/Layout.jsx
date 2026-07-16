import { Outlet } from "react-router-dom";
import Sidebar from "./Sidebar";

export default function Layout() {
  return (
    <div className="h-screen p-4 overflow-hidden">
      <Sidebar />
      <main className="ml-64 pl-4 h-full">
        <div className="bg-white rounded-2xl shadow-sm p-8 h-full overflow-y-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}