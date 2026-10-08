const TOKEN_KEY = "pc_access_token";
const USER_KEY = "pc_user";
const THEME_KEY = "pc_theme";

const loginView = document.getElementById("login-view");
const appView = document.getElementById("app-view");
const loginForm = document.getElementById("login-form");
const loginError = document.getElementById("login-error");
const productForm = document.getElementById("product-form");
const productStatus = document.getElementById("product-status");
const productsError = document.getElementById("products-error");
const productsTbody = document.getElementById("products-tbody");
const userLabel = document.getElementById("user-label");

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function formatPrice(value) {
  return new Intl.NumberFormat("pl-PL", {
    style: "currency",
    currency: "PLN"
  }).format(Number(value));
}

function getToken() {
  return sessionStorage.getItem(TOKEN_KEY);
}

function getUser() {
  const raw = sessionStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

function setSession(tokenInfo) {
  sessionStorage.setItem(TOKEN_KEY, tokenInfo.accessToken);
  sessionStorage.setItem(
    USER_KEY,
    JSON.stringify({
      email: tokenInfo.email,
      displayName: tokenInfo.displayName
    })
  );
}

function clearSession() {
  sessionStorage.removeItem(TOKEN_KEY);
  sessionStorage.removeItem(USER_KEY);
}

function createIdempotencyKey() {
  if (crypto.randomUUID) {
    return crypto.randomUUID().replaceAll("-", "");
  }
  return `${Date.now()}${Math.random().toString(16).slice(2)}`;
}

async function api(path, options = {}) {
  const headers = new Headers(options.headers || {});
  headers.set("Accept", "application/json");

  if (options.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const token = getToken();
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(path, { ...options, headers });
  const text = await response.text();
  let payload = null;
  if (text) {
    try {
      payload = JSON.parse(text);
    } catch {
      payload = null;
    }
  }

  if (response.status === 401) {
    clearSession();
    showLogin("Sesja wygasła. Zaloguj się ponownie.");
    throw new Error("Unauthorized");
  }

  return { response, payload };
}

function showLogin(message) {
  appView.hidden = true;
  loginView.hidden = false;
  if (message) {
    loginError.hidden = false;
    loginError.textContent = message;
  }
}

function showApp() {
  const user = getUser();
  loginView.hidden = true;
  appView.hidden = false;
  userLabel.textContent = user
    ? `${user.displayName} (${user.email})`
    : "Zalogowany użytkownik";
  loadProducts();
}

function applyTheme(theme) {
  document.documentElement.setAttribute("data-theme", theme);
  localStorage.setItem(THEME_KEY, theme);
  const label = theme === "dark" ? "☀️ Jasny" : "🌙 Ciemny";
  const compact = theme === "dark" ? "☀️" : "🌙";
  const loginToggle = document.getElementById("theme-toggle");
  const appToggle = document.getElementById("theme-toggle-app");
  if (loginToggle) loginToggle.textContent = label;
  if (appToggle) appToggle.textContent = compact;
}

function toggleTheme() {
  const current = document.documentElement.getAttribute("data-theme") || "light";
  applyTheme(current === "dark" ? "light" : "dark");
}

function renderProducts(products) {
  if (!products || products.length === 0) {
    productsTbody.innerHTML = `<tr><td colspan="3" class="empty">Brak produktów w katalogu.</td></tr>`;
    return;
  }

  productsTbody.innerHTML = products
    .map(
      (p) => `
      <tr>
        <td>${escapeHtml(p.kod)}</td>
        <td>${escapeHtml(p.nazwa)}</td>
        <td>${escapeHtml(formatPrice(p.cena))}</td>
      </tr>`
    )
    .join("");
}

async function loadProducts() {
  productsError.hidden = true;
  productsTbody.innerHTML = `<tr><td colspan="3" class="empty">Ładowanie…</td></tr>`;

  try {
    const { response, payload } = await api("/api/products");
    if (!response.ok || !payload?.isSuccess) {
      throw new Error(payload?.errorMessage || "Nie udało się pobrać listy produktów.");
    }
    renderProducts(payload.data || []);
  } catch (err) {
    if (err.message === "Unauthorized") return;
    productsError.hidden = false;
    productsError.textContent = err.message || "Błąd pobierania produktów.";
    productsTbody.innerHTML = `<tr><td colspan="3" class="empty">Błąd ładowania.</td></tr>`;
  }
}

loginForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  loginError.hidden = true;

  const email = document.getElementById("login-email").value.trim();
  const password = document.getElementById("login-password").value;
  const submit = document.getElementById("login-submit");
  submit.disabled = true;
  submit.textContent = "Logowanie...";

  try {
    const { response, payload } = await api("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password })
    });

    if (!response.ok || !payload?.isSuccess || !payload.data?.accessToken) {
      throw new Error(payload?.errorMessage || "Nieprawidłowy email lub hasło.");
    }

    setSession(payload.data);
    loginForm.reset();
    showApp();
  } catch (err) {
    loginError.hidden = false;
    loginError.textContent = err.message || "Błąd logowania.";
  } finally {
    submit.disabled = false;
    submit.textContent = "Zaloguj";
  }
});

productForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  productStatus.hidden = true;

  const kod = document.getElementById("product-kod").value.trim();
  const nazwa = document.getElementById("product-nazwa").value.trim();
  const cena = Number(document.getElementById("product-cena").value);
  const submit = document.getElementById("product-submit");
  submit.disabled = true;

  try {
    const { response, payload } = await api("/api/products", {
      method: "POST",
      headers: {
        "Idempotency-Key": createIdempotencyKey()
      },
      body: JSON.stringify({ kod, nazwa, cena })
    });

    if (!response.ok || !payload?.isSuccess) {
      throw new Error(payload?.errorMessage || "Nie udało się dodać produktu.");
    }

    productStatus.hidden = false;
    productStatus.className = "status success";
    productStatus.textContent = `Dodano produkt ${payload.data.kod}.`;
    productForm.reset();
    await loadProducts();
  } catch (err) {
    if (err.message === "Unauthorized") return;
    productStatus.hidden = false;
    productStatus.className = "status error";
    productStatus.textContent = err.message || "Błąd dodawania produktu.";
  } finally {
    submit.disabled = false;
  }
});

document.getElementById("logout-btn").addEventListener("click", () => {
  clearSession();
  showLogin();
});

document.getElementById("refresh-btn").addEventListener("click", () => loadProducts());
document.getElementById("theme-toggle").addEventListener("click", toggleTheme);
document.getElementById("theme-toggle-app").addEventListener("click", toggleTheme);

applyTheme(localStorage.getItem(THEME_KEY) || "light");

if (getToken()) {
  showApp();
} else {
  showLogin();
}
