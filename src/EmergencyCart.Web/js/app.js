const API_BASE_URL = "https://localhost:7085";

let currentCartId = null;
let currentRole = null;
let authToken = null;
let currentInspectionId = null;
let currentInspectionStatus = null; // "InProgress" | "Completed"
let conferenceCarts = [];
let toastTimer = null;

class SessionExpiredError extends Error {}

class ApiError extends Error {
    constructor(status) {
        super(`Falha na requisição (HTTP ${status})`);
        this.status = status;
    }
}

// ============ FETCH AUTENTICADO (adiciona o token e trata 401 sozinho) ============
async function apiFetch(url, options = {}) {
    const response = await fetch(url, {
        ...options,
        headers: {
            ...(options.headers || {}),
            "Authorization": `Bearer ${authToken}`
        }
    });

    if (response.status === 401) {
        logout();
        toast("Sessão expirada. Faça login novamente.");
        throw new SessionExpiredError();
    }

    return response;
}

function logout() {
    authToken = null;
    currentRole = null;
    document.getElementById("app").classList.add("hidden");
    document.getElementById("login-screen").classList.remove("hidden");
    resetConferencePanel();
}

function showPage(page) {
    document.querySelectorAll(".page").forEach(p => p.classList.remove("active-page"));
    document.getElementById(`page-${page}`).classList.add("active-page");

    document.querySelectorAll(".nav-item").forEach(btn => {
        btn.classList.toggle("active", btn.dataset.page === page);
    });

    const titles = {
        dashboard: "Dashboard",
        sectors: "Setores",
        items: "Items",
        carts: "Carrinhos",
        "cart-config": "Configurar carrinho",
        conference: "Conferência",
        employees: "Funcionários"
    };
    document.getElementById("page-title").textContent = titles[page];

    if (page === "conference") loadConferencePage();
}

// ============ PERMISSÕES POR PERFIL (front-end só esconde botão; quem protege de verdade é o [Authorize] do backend) ============
function applyRolePermissions() {
    const isAdmin = currentRole === "Admin";
    const isPharmacist = currentRole === "Pharmacist";
    const isNurse = currentRole === "Nurse";
    const canDoConference = isNurse || isPharmacist;

    document.getElementById("new-sector").style.display = isAdmin ? "" : "none";
    document.getElementById("new-items").style.display = isAdmin ? "" : "none";
    document.getElementById("new-EmergencyCart").style.display = isPharmacist ? "" : "none";

    const employeesNav = document.querySelector('.nav-item[data-page="employees"]');
    if (employeesNav) employeesNav.style.display = isAdmin ? "" : "none";

    const conferenceNav = document.querySelector('.nav-item[data-page="conference"]');
    if (conferenceNav) conferenceNav.style.display = canDoConference ? "" : "none";

    const newConferenceBtn = document.querySelector('.welcome [data-page="conference"]');
    if (newConferenceBtn) newConferenceBtn.style.display = canDoConference ? "" : "none";
}


const roleLabels = {
    Admin: "Administrador",
    Pharmacist: "Farmacêutico",
    Nurse: "Enfermeiro"
};

function renderDashboard(carts) {
    document.querySelector("#page-dashboard .stat-card:nth-child(1) strong").textContent = carts.length;

    const uniqueSectors = new Set(carts.map(c => c.sectorId)).size;
    document.querySelector("#page-dashboard .stat-card:nth-child(1) small").textContent = `Em ${uniqueSectors} setores`;
}

function renderSectors(sectorsList, cartsList, filter = "") {
    const tbody = document.getElementById("sector-table");
    tbody.innerHTML = sectorsList
        .filter(s => s.name.toLowerCase().includes(filter.toLowerCase()))
        .map(s => {
            const cartsInSector = cartsList.filter(c => c.sectorId === s.id).length;
            const isActive = s.isActive ?? true;
            return `
                <tr>
                    <td><strong>${s.name}</strong></td>
                    <td>${cartsInSector}</td>
                    <td style="color:#8792a3;">—</td>
                    <td><span class="badge ${isActive ? "badge-green" : "badge-orange"}">${isActive ? "Ativo" : "Inativo"}</span></td>
                    <td><button class="text-btn" onclick="showSectorCarts('${s.name}')">Detalhes</button></td>
                </tr>
            `;
        }).join("");
}

function renderCarts(list) {
    const canConfigure = currentRole === "Pharmacist";

    document.getElementById("cart-grid").innerHTML = list.map(c => `
        <div class="cart-card">
            <div class="cart-status">
                <span class="badge badge-green">Ativo</span>
            </div>
            <h3>Carrinho ${c.code}</h3>
            <p>${c.sectorName}</p>
            <div class="cart-meta">
                <span>${c.itemsCount} itens cadastrados</span>
            </div>

            ${canConfigure
                ? `<button class="btn btn-primary" onclick="configureCart('${c.id}')">Configurar itens</button>`
                : `<button class="btn" disabled title="Somente o farmacêutico configura itens">Configurar itens</button>`
            }
        </div>
    `).join("");
}

function toast(message) {
    const el = document.getElementById("toast");
    el.textContent = message;
    el.classList.add("show");
    clearTimeout(toastTimer); // evita que o aviso anterior esconda o novo antes da hora
    toastTimer = setTimeout(() => el.classList.remove("show"), 2600);
}

async function showSectorCarts(sectorName) {
    const carts = await getCarts();
    const filtered = carts.filter(c => c.sectorName === sectorName);
    renderCarts(filtered);
    showPage("carts");
    toast(`Mostrando carrinhos de ${sectorName}`);
}

async function createSector(name) {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/sector`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: name })
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }

        toast(`Setor "${name}" criado com sucesso!`);
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast("Erro ao criar setor.");
    }
}

async function createEmergencyCart(sectorCode) {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/EmergencyCart/${sectorCode}`, {
            method: "POST"
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }

        toast("Carrinho gerado e alocado com sucesso!");
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast("Erro ao criar carrinho de emergência.");
    }
}

async function getEmployees() {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/employee`);
        if (!response.ok) throw new Error("Erro ao buscar funcionários.");
        return await response.json();
    } catch (error) {
        if (error instanceof SessionExpiredError) return [];
        console.error(error);
        toast("Erro ao carregar funcionários.");
        return [];
    }
}

function renderEmployees(list) {
    const tbody = document.getElementById("employees-table");
    tbody.innerHTML = list.map(e => `
        <tr>
            <td><strong>${e.firstName} ${e.lastName}</strong></td>
            <td>${e.email}</td>
            <td>${roleLabels[e.role] ?? e.role}</td>
        </tr>
    `).join("");
}

async function createEmployee(payload) {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/employee`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }

        toast("Funcionário cadastrado com sucesso!");
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast("Erro ao cadastrar funcionário.");
    }
}

async function createItem(name, category) {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/Item`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: name, category: category })
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }

        toast(`Item "${name}" com a categoria "${category}" foi criado com sucesso!`);
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast("Erro ao criar item.");
    }
}

async function createCartItem(emergencyCartId, itemId, expectedQuantity) {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/CartItem/${emergencyCartId}`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ itemId: itemId, expectedQuantity: expectedQuantity })
        });

        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }

        toast("Item adicionado ao carrinho com sucesso!");

        const cartItems = await getCartItems(emergencyCartId);
        renderCartItems(cartItems);
        await loadItemsSelect(cartItems);
        document.getElementById("cart-item-quantity").value = 1;
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast("Erro ao adicionar item ao carrinho.");
    }
}

async function getItems() {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/item`);
        if (!response.ok) throw new Error("Erro ao buscar itens.");
        return await response.json();
    } catch (error) {
        if (error instanceof SessionExpiredError) return [];
        console.error(error);
        toast("Erro ao carregar itens.");
        return [];
    }
}

async function getSectors() {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/sector`);
        if (!response.ok) throw new Error("Erro ao buscar setores.");
        return await response.json();
    } catch (error) {
        if (error instanceof SessionExpiredError) return [];
        console.error(error);
        toast("Erro ao carregar setores.");
        return [];
    }
}

async function getCarts() {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/EmergencyCart`);
        if (!response.ok) throw new Error("Erro ao buscar carrinhos.");
        return await response.json();
    } catch (error) {
        if (error instanceof SessionExpiredError) return [];
        console.error(error);
        toast("Erro ao carregar carrinhos.");
        return [];
    }
}

async function getCartItems(cartId) {
    try {
        const response = await apiFetch(`${API_BASE_URL}/api/CartItem/${cartId}`);
        if (!response.ok) throw new Error("Erro ao buscar itens do carrinho.");
        return await response.json();
    } catch (error) {
        if (error instanceof SessionExpiredError) return [];
        console.error(error);
        toast("Erro ao carregar itens do carrinho.");
        return [];
    }
}

async function loadItemsSelect(cartItems = []) {
    const items = await getItems();
    const select = document.getElementById("cart-item-select");
    select.innerHTML = `<option value="">Selecione um item</option>`;

    const configuredItemIds = cartItems.map(cartItem => cartItem.itemId);

    items
        .filter(item => !configuredItemIds.includes(item.id))
        .forEach(item => {
            const option = document.createElement("option");
            option.value = item.id;
            option.textContent = `${item.name}`;
            select.appendChild(option);
        });
}

function renderCartItems(cartItems) {
    const container = document.getElementById("cart-items-list");

    if (cartItems.length === 0) {
        container.innerHTML = `<p style="color:#8792a3;">Nenhum item configurado neste carrinho.</p>`;
        return;
    }

    container.innerHTML = cartItems.map(cartItem => `
        <div class="checklist-item">
            <div>
                <strong>${cartItem.itemName}</strong>
                <small>${cartItem.category}</small>
            </div>
            <strong>Quantidade esperada: ${cartItem.expectedQuantity}</strong>
        </div>
    `).join("");
}

// AJUSTE AQUI se os nomes não baterem com seu enum ItemCategory
const categoryLabels = {
    Medication: "Medicação",
    Material: "Material",
    Equipment: "Equipamento"
};

function renderItems(list, filter = "") {
    const tbody = document.getElementById("items-table");
    tbody.innerHTML = list
        .filter(i => i.name.toLowerCase().includes(filter.toLowerCase()))
        .map(i => `
            <tr>
                <td><strong>${i.name}</strong></td>
                <td>${categoryLabels[i.category] ?? i.category}</td>
                <td></td>
            </tr>
        `).join("");
}


async function openModal(title, fields) {
    return new Promise((resolve) => {
        const modal = document.getElementById("custom-modal");
        document.getElementById("modal-title").textContent = title;

        const inputsContainer = document.getElementById("modal-inputs");
        inputsContainer.innerHTML = "";

        fields.forEach(field => {
            if (field.type === "select") {
                const options = field.options.map(opt => `<option value="${opt.id}">${opt.name}</option>`).join("");
                inputsContainer.innerHTML += `
                    ${field.label ? `<label>${field.label}</label>` : ""}
                    <select id="${field.id}" class="modal-input">${options}</select>
                `;
            } else {
                inputsContainer.innerHTML += `
                    ${field.label ? `<label>${field.label}</label>` : ""}
                    <input type="${field.type}" id="${field.id}" placeholder="${field.placeholder ?? ""}" class="modal-input">
                `;
            }
        });

        const btnConfirm = document.getElementById("modal-confirm");
        const btnCancel = document.getElementById("modal-cancel");

        const newConfirm = btnConfirm.cloneNode(true);
        const newCancel = btnCancel.cloneNode(true);
        btnConfirm.replaceWith(newConfirm);
        btnCancel.replaceWith(newCancel);

        // ============ LÓGICA DE VALIDAÇÃO DE SENHA NO MODAL ============
        const passwordInput = document.getElementById("password");
        const confirmInput = document.getElementById("confirmPassword");

        if (passwordInput && confirmInput) {
            // Estado inicial: se houver campos de senha, começa desabilitado até preencher certo
            newConfirm.disabled = true;
            newConfirm.style.opacity = "0.5"; // Visual opcional para indicar desativado

            const validatePasswords = () => {
                const pass = passwordInput.value;
                const confirm = confirmInput.value;

                if (pass && confirm && pass === confirm) {
                    newConfirm.disabled = false;
                    newConfirm.style.opacity = "1";
                    confirmInput.style.borderColor = "#22c55e"; // Verde
                } else {
                    newConfirm.disabled = true;
                    newConfirm.style.opacity = "0.5";
                    if (confirm) confirmInput.style.borderColor = "#ef4444"; // Vermelho
                }
            };

            passwordInput.addEventListener("input", validatePasswords);
            confirmInput.addEventListener("input", validatePasswords);
        }
        // ==============================================================

        newConfirm.addEventListener("click", () => {
            if (newConfirm.disabled) return; // Segurança extra
            const results = {};
            fields.forEach(f => results[f.id] = document.getElementById(f.id).value);
            modal.close();
            resolve(results);
        });

        newCancel.addEventListener("click", () => {
            modal.close();
            resolve(null);
        });

        modal.showModal();
    });
}


async function configureCart(cartId) {
    currentCartId = cartId;
    showPage("cart-config");

    const cartItems = await getCartItems(cartId);
    await loadItemsSelect(cartItems);
    renderCartItems(cartItems);
}

async function initApp() {
    const sectors = await getSectors();
    const carts = await getCarts();

    renderSectors(sectors, carts);
    renderCarts(carts);
    renderDashboard(carts);

    const items = await getItems();
    renderItems(items);
}

// ============ LOGIN REAL (chama a API, guarda o token) ============
document.getElementById("login-form").addEventListener("submit", async (e) => {
    e.preventDefault();
    const email = document.getElementById("login-email").value;
    const password = document.getElementById("login-password").value;

    try {
        const response = await fetch(`${API_BASE_URL}/api/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) {
            toast("E-mail ou senha inválidos.");
            return;
        }

        const data = await response.json();
        authToken = data.token;
        currentRole = data.role;

        document.getElementById("user-name").textContent = data.firstName;
        document.getElementById("user-role").textContent = roleLabels[currentRole] ?? currentRole;

        document.getElementById("login-screen").classList.add("hidden");
        document.getElementById("app").classList.remove("hidden");
        showPage("dashboard");

        applyRolePermissions();
        initApp();
    } catch (error) {
        console.error(error);
        toast("Erro ao entrar. Verifique sua conexão com a API.");
    }
});

document.getElementById("logout").addEventListener("click", logout);

document.querySelectorAll("[data-page]").forEach(btn => {
    btn.addEventListener("click", () => showPage(btn.dataset.page));
});

document.querySelector('.nav-item[data-page="carts"]')?.addEventListener("click", async () => {
    const carts = await getCarts();
    renderCarts(carts);
});

document.querySelector('.nav-item[data-page="employees"]')?.addEventListener("click", async () => {
    const employees = await getEmployees();
    renderEmployees(employees);
});

document.getElementById("sector-search").addEventListener("input", async e => {
    const sectors = await getSectors();
    const carts = await getCarts();
    renderSectors(sectors, carts, e.target.value);
});

document.getElementById("new-sector").addEventListener("click", async () => {
    const result = await openModal("Novo setor", [
        { id: "sectorName", type: "text", placeholder: "Ex: Centro Cirúrgico" }
    ]);
    if (!result || !result.sectorName?.trim()) return;

    await createSector(result.sectorName.trim());

    const sectors = await getSectors();
    const carts = await getCarts();
    renderSectors(sectors, carts);
    renderDashboard(carts);
});

document.getElementById("new-EmergencyCart").addEventListener("click", async () => {
    const sectors = await getSectors();
    const carts = await getCarts();

    if (!sectors || sectors.length === 0) {
        toast("Cadastre um setor antes de criar um carrinho.");
        return;
    }

    const availableSectors = sectors.filter(sector =>
        !carts.some(cart => cart.sectorId === sector.id)
    );

    if (availableSectors.length === 0) {
        toast("Todos os setores atuais já possuem um carrinho de emergência!");
        return;
    }

    const result = await openModal("Novo carrinho de emergência", [
        {
            id: "sectorCode",
            type: "select",
            label: "Selecione o setor:",
            options: availableSectors.map(s => ({ id: s.code, name: s.name }))
        }
    ]);

    if (result && result.sectorCode) {
        await createEmergencyCart(result.sectorCode);

        const updatedCarts = await getCarts();

        renderCarts(updatedCarts);
        renderDashboard(updatedCarts);

        const updatedSectors = await getSectors();
        renderSectors(updatedSectors, updatedCarts);
    }
});

document.getElementById("new-items").addEventListener("click", async () => {
    const result = await openModal("Novo item", [
        { id: "itemName", type: "text", placeholder: "Ex: Adrenalina 1mg/ml" },
        {
            id: "itemCategory",
            type: "select",
            label: "Categoria",
            options: [
                { id: "Medication", name: "Medicação" },
                { id: "Material", name: "Material" },
                { id: "Equipment", name: "Equipamento" }
            ]
        }
    ]);

    if (!result || !result.itemName?.trim() || !result.itemCategory) return;

    await createItem(result.itemName.trim(), result.itemCategory);

    const items = await getItems();
    renderItems(items);
});

document.getElementById("items-search").addEventListener("input", async e => {
    const items = await getItems();
    renderItems(items, e.target.value);
});

document.getElementById("new-employee").addEventListener("click", async () => {
    const result = await openModal("Novo funcionário", [
        { id: "firstName", type: "text", placeholder: "Nome" },
        { id: "lastName", type: "text", placeholder: "Sobrenome" },
        { id: "email", type: "email", placeholder: "E-mail" },
        { id: "password", type: "password", placeholder: "Senha provisória" },
        { id: "confirmPassword", type: "password", placeholder: "Confirmar senha" },
        {
            id: "role",
            type: "select",
            label: "Perfil",
            options: [
                { id: "Admin", name: "Administrador (TI)" },
                { id: "Pharmacist", name: "Farmacêutico" },
                { id: "Nurse", name: "Enfermeiro" }
            ]
        }
    ]);

    if (!result || !result.firstName?.trim() || !result.lastName?.trim() || !result.email?.trim() || !result.password) return;

    // Validação final de segurança caso ele tenha ignorado os avisos visuais
    if (result.password !== result.confirmPassword) {
        toast("As senhas não coincidem.");
        return;
    }

    await createEmployee({
        firstName: result.firstName.trim(),
        lastName: result.lastName.trim(),
        email: result.email.trim(),
        password: result.password,
        role: result.role
    });

    const employees = await getEmployees();
    renderEmployees(employees);
});

// ============ VALIDAÇÃO ASSÍNCRONA EM TEMPO REAL NAS SENHAS ============
document.addEventListener("input", (e) => {
    // Verifica se os inputs de senha existem no modal atual
    const passwordInput = document.getElementById("password");
    const confirmInput = document.getElementById("confirmPassword");

    if (!passwordInput || !confirmInput) return;

    // Se o usuário estiver digitando em um dos dois campos de senha
    if (e.target === passwordInput || e.target === confirmInput) {
        const pass = passwordInput.value;
        const confirm = confirmInput.value;

        // Se o campo de confirmação estiver vazio, remove qualquer destaque
        if (!confirm) {
            confirmInput.style.borderColor = "";
            return;
        }

        // Se forem iguais, pinta a borda de verde; se diferentes, de vermelho
        if (pass === confirm) {
            confirmInput.style.borderColor = "#22c55e"; // Verde (Sucesso)
        } else {
            confirmInput.style.borderColor = "#ef4444"; // Vermelho (Erro)
        }
    }
});

document.getElementById("add-cart-item").addEventListener("click", async () => {
    const itemId = document.getElementById("cart-item-select").value;
    const quantity = Number(document.getElementById("cart-item-quantity").value);

    if (!currentCartId) {
        toast("Nenhum carrinho selecionado.");
        return;
    }
    if (!itemId) {
        toast("Selecione um item.");
        return;
    }
    if (quantity < 1 || quantity > 5) {
        toast("A quantidade deve estar entre 1 e 5.");
        return;
    }

    await createCartItem(currentCartId, itemId, quantity);
});


// ============ CONFERÊNCIA (AC2) ============
const inspectionStatusLabels = {
    InProgress: "Em andamento",
    Completed: "Finalizada"
};

const comparisonLabels = {
    NotChecked: { text: "Não conferido", css: "badge-blue" },
    Matched: { text: "Confere", css: "badge-green" },
    Missing: { text: "Faltando", css: "badge-orange" },
    Surplus: { text: "Sobra", css: "badge-blue" }
};

const conferenceErrors = {
    start: {
        400: "Não foi possível iniciar: confirme se o carrinho está ativo e possui itens configurados.",
        403: "Seu perfil não tem permissão para iniciar conferências.",
        404: "Carrinho não encontrado.",
        409: "Já existe uma conferência em andamento para este carrinho.",
        fallback: "Não foi possível iniciar a conferência."
    },
    load: {
        403: "Você não tem permissão para ver esta conferência.",
        404: "Conferência não encontrada.",
        fallback: "Não foi possível carregar os itens. Clique em Iniciar conferência para tentar de novo."
    },
    register: {
        400: "Quantidade inválida. Confira o valor informado.",
        403: "Você não tem permissão para esta ação. Só quem iniciou a conferência pode alterá-la.",
        404: "Conferência ou item não encontrado.",
        409: "Esta conferência já foi finalizada.",
        fallback: "Não foi possível registrar a quantidade."
    },
    complete: {
        400: "Registre a quantidade de todos os itens antes de finalizar.",
        403: "Você não tem permissão para esta ação. Só quem iniciou a conferência pode finalizá-la.",
        404: "Conferência não encontrada.",
        409: "Esta conferência já foi finalizada.",
        fallback: "Não foi possível finalizar a conferência."
    }
};

function conferenceErrorMessage(action, error) {
    const messages = conferenceErrors[action];
    if (error instanceof ApiError) return messages[error.status] ?? messages.fallback;
    return "Erro de comunicação com o servidor.";
}

// ---- chamadas à API ----
async function requestOrThrow(url, options) {
    const response = await apiFetch(url, options);
    if (!response.ok) throw new ApiError(response.status);
    return response;
}

async function startInspection(emergencyCartId) {
    const response = await requestOrThrow(
        `${API_BASE_URL}/api/inspection/${encodeURIComponent(emergencyCartId)}`,
        { method: "POST" }
    );
    const data = await response.json();
    if (!data?.id) throw new Error("Resposta inesperada ao iniciar a conferência.");
    return data.id;
}

async function getInspectionComparison(inspectionId) {
    const response = await requestOrThrow(
        `${API_BASE_URL}/api/inspection/${encodeURIComponent(inspectionId)}/comparison`
    );
    return await response.json();
}

async function registerFoundQuantity(inspectionId, itemId, foundQuantity) {
    await requestOrThrow(
        `${API_BASE_URL}/api/inspection/${encodeURIComponent(inspectionId)}/items/${encodeURIComponent(itemId)}`,
        {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ foundQuantity })
        }
    );
}

async function completeInspection(inspectionId) {
    await requestOrThrow(
        `${API_BASE_URL}/api/inspection/${encodeURIComponent(inspectionId)}/complete`,
        { method: "PATCH" }
    );
}

async function getInProgressInspectionId(emergencyCartId) {
    const response = await requestOrThrow(
        `${API_BASE_URL}/api/inspection/in-progress/${encodeURIComponent(emergencyCartId)}`
    );
    const data = await response.json();
    if (!data?.inspectionId) throw new Error("Resposta inesperada ao buscar a conferência em andamento.");
    return data.inspectionId;
}

// Tenta iniciar; se já existir uma em andamento, retoma essa (só funciona para quem a iniciou).
// Devolve { inspectionId, resumed } para a tela saber qual mensagem mostrar.
async function startOrResumeInspection(emergencyCartId) {
    try {
        const inspectionId = await startInspection(emergencyCartId);
        return { inspectionId, resumed: false };
    } catch (error) {
        if (!(error instanceof ApiError) || error.status !== 409) throw error;

        try {
            const inspectionId = await getInProgressInspectionId(emergencyCartId);
            return { inspectionId, resumed: true };
        } catch (resumeError) {
            if (resumeError instanceof SessionExpiredError) throw resumeError;
            throw error; // mantém a mensagem original do 409
        }
    }
}

// ---- estado e seleção ----
function resetConferencePanel() {
    currentInspectionId = null;
    currentInspectionStatus = null;
    document.getElementById("checklist").replaceChildren();
    document.getElementById("conference-checklist-panel").classList.add("hidden");
    document.getElementById("finish-conference").disabled = true;
    document.getElementById("start-conference").disabled = false;
    document.getElementById("conference-sector").disabled = false;
    document.getElementById("conference-cart").disabled = false;
}

function populateConferenceCarts(sectorId) {
    const cartSelect = document.getElementById("conference-cart");
    cartSelect.replaceChildren(new Option("Selecione o carrinho", ""));

    conferenceCarts
        .filter(cart => cart.sectorId === sectorId)
        .forEach(cart => cartSelect.appendChild(new Option(`Carrinho ${cart.code}`, cart.id)));

    // com um único carrinho no setor, já deixa selecionado
    if (cartSelect.options.length === 2) cartSelect.selectedIndex = 1;
}

function populateConferenceSectors() {
    const sectorSelect = document.getElementById("conference-sector");
    const sectors = new Map(conferenceCarts.map(cart => [cart.sectorId, cart.sectorName]));

    sectorSelect.replaceChildren(new Option("Selecione o setor", ""));
    sectors.forEach((name, id) => sectorSelect.appendChild(new Option(name, id)));
    populateConferenceCarts("");
}

async function loadConferencePage() {
    if (currentInspectionStatus === "InProgress") return; // não recarrega no meio de uma conferência

    resetConferencePanel();
    const carts = await getCarts();
    conferenceCarts = carts.filter(cart => cart.itemsCount > 0);
    populateConferenceSectors();

    if (conferenceCarts.length === 0) toast("Nenhum carrinho com itens configurados.");
}

// ---- renderização ----
function setComparisonBadge(badge, comparison) {
    const status = comparisonLabels[comparison] ?? { text: comparison, css: "badge-blue" };
    badge.className = `badge ${status.css}`;
    badge.textContent = status.text;
}

function updatePendingCount(items) {
    const notChecked = items.filter(item => item.comparison === "NotChecked").length;
    document.getElementById("pending-count").textContent =
        `${notChecked} ${notChecked === 1 ? "item sem registro" : "itens sem registro"}`;
}

function buildChecklistRow(item, canEdit) {
    const row = document.createElement("div");
    row.className = "checklist-item";

    const info = document.createElement("div");
    const name = document.createElement("strong");
    name.textContent = item.itemName;
    const expected = document.createElement("small");
    expected.textContent = describeQuantities(item); 
    info.append(name, expected);

    const input = document.createElement("input");
    input.className = "qty";
    input.type = "number";
    input.min = "0";
    input.step = "1";
    input.value = item.foundQuantity ?? "";
    input.disabled = !canEdit;
    input.setAttribute("aria-label", `Quantidade encontrada de ${item.itemName}`);

    const group = document.createElement("div");
    group.className = "qty-group";
    group.append(input);

    if (canEdit) {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "btn";
        button.textContent = "Registrar";
        button.setAttribute("aria-label", `Registrar quantidade de ${item.itemName}`);
        button.addEventListener("click", () => handleRegisterItem(item.itemId, input, button));
        input.addEventListener("keydown", e => {
            if (e.key === "Enter") button.click();
        });
        group.append(button);
    }

    const badge = document.createElement("span");
    setComparisonBadge(badge, item.comparison);

    row.append(info, group, badge);
    return row;
}

function renderConference(data) {
    const inProgress = data.status === "InProgress";
    currentInspectionStatus = data.status;

    document.getElementById("conference-status-badge").textContent =
        inspectionStatusLabels[data.status] ?? data.status;

    const sortedItems = [...data.items].sort((a, b) => a.itemName.localeCompare(b.itemName, "pt-BR"));
    document.getElementById("checklist")
        .replaceChildren(...sortedItems.map(item => buildChecklistRow(item, inProgress)));

    updatePendingCount(data.items);

    document.getElementById("finish-conference").disabled = !inProgress;
    document.getElementById("start-conference").disabled = inProgress;
    document.getElementById("conference-sector").disabled = inProgress;
    document.getElementById("conference-cart").disabled = inProgress;
    document.getElementById("conference-checklist-panel").classList.remove("hidden");
}

function describeQuantities(item) {
    const base = `Esperado: ${item.expectedQuantity}`;

    if (item.comparison === "Missing") {
        const n = item.missingQuantity;
        return `${base}, ${n === 1 ? "falta 1" : `faltam ${n}`}`;
    }
    if (item.comparison === "Surplus") {
        const n = item.difference;
        return `${base}, ${n === 1 ? "sobra 1" : `sobram ${n}`}`;
    }
    return base;
}


// ---- ações do usuário ----
async function handleStartConference() {
    const cartId = document.getElementById("conference-cart").value;
    if (!cartId) {
        toast("Selecione o setor e o carrinho.");
        return;
    }

    const startButton = document.getElementById("start-conference");
    startButton.disabled = true;

    try {
        let resumed = false;

        // se a conferência já foi criada mas os itens não carregaram, só tenta carregar de novo
        if (!currentInspectionId) {
            const result = await startOrResumeInspection(cartId);
            currentInspectionId = result.inspectionId;
            resumed = result.resumed;
        }

        const data = await getInspectionComparison(currentInspectionId);
        renderConference(data);
        toast(resumed ? "Conferência em andamento retomada." : "Conferência iniciada.");
    } catch (error) {
        startButton.disabled = false;
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast(conferenceErrorMessage(currentInspectionId ? "load" : "start", error));
    }
}

async function refreshRowStatus(itemId, input) {
   try {
        const data = await getInspectionComparison(currentInspectionId);
        const updated = data.items.find(item => item.itemId === itemId);
        const row = input.closest(".checklist-item");
        const badge = row?.querySelector(".badge");
        const details = row?.querySelector("small");

        if (updated && badge) setComparisonBadge(badge, updated.comparison);
        if (updated && details) details.textContent = describeQuantities(updated);
        updatePendingCount(data.items);
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast("Quantidade registrada, mas não foi possível atualizar o status na tela.");
    }
}

async function handleRegisterItem(itemId, input, button) {
    if (!currentInspectionId) return;

    const rawValue = input.value.trim();
    const quantity = Number(rawValue);
    // campo vazio não pode virar 0: "não conferido" e "conferi e não tem nenhum" são coisas diferentes
    if (rawValue === "" || !Number.isInteger(quantity) || quantity < 0) {
        toast("Informe uma quantidade inteira, maior ou igual a zero.");
        return;
    }

    button.disabled = true;
    try {
        await registerFoundQuantity(currentInspectionId, itemId, quantity);
        toast("Quantidade registrada.");
        await refreshRowStatus(itemId, input);
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast(conferenceErrorMessage("register", error));
    } finally {
        button.disabled = false;
    }
}

async function handleFinishConference() {
    if (!currentInspectionId) {
        toast("Inicie uma conferência antes de finalizar.");
        return;
    }

    const finishButton = document.getElementById("finish-conference");
    finishButton.disabled = true;

    try {
        await completeInspection(currentInspectionId);
        currentInspectionStatus = "Completed";
    } catch (error) {
        finishButton.disabled = false;
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast(conferenceErrorMessage("complete", error));
        return;
    }

    try {
        const data = await getInspectionComparison(currentInspectionId);
        renderConference(data);
        const missing = data.items.filter(item => item.comparison === "Missing").length;
        toast(missing
            ? `Conferência finalizada com ${missing} ${missing === 1 ? "item faltando" : "itens faltando"}.`
            : "Conferência finalizada com sucesso!");
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast("Conferência finalizada, mas não foi possível carregar o resultado.");
    }
}

document.getElementById("conference-sector").addEventListener("change", e => {
    resetConferencePanel();
    populateConferenceCarts(e.target.value);
});

document.getElementById("conference-cart").addEventListener("change", resetConferencePanel);
document.getElementById("start-conference").addEventListener("click", handleStartConference);
document.getElementById("finish-conference").addEventListener("click", handleFinishConference);
