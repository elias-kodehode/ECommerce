import { useEffect } from "react";

export default function App() {
  async function submit(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    const data = new FormData(e.target);
    const email = data.get("email");
    const password = data.get("password");

    let response = await fetch("api/users/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
      headers: {
        "content-type": "application/json",
      },
    });
  }
  return (
    <>
      <form onSubmit={submit}>
        <input placeholder="email" type="email" name="email" />
        <input placeholder="password" type="password" name="password" />
        <button type="submit">Submit</button>
      </form>
      <button
        type="button"
        onClick={async () => {
          await fetch("api/users/logout", {
            method: "POST",
            credentials: "include",
          });
          window.location.href = "/";
        }}
      >
        Logout
      </button>

      <button
        type="button"
        onClick={async () => {
          const response = await fetch("api/users/me", {
            method: "GET",
            credentials: "include",
          });

          if (!response.ok) {
            return;
          }
          console.log(response);
          const result = await response.json();
          console.log("ME: ", result);
        }}
      >
        Me
      </button>
    </>
  );
}
