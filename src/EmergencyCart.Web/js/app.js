const API_BASE_URL = "https://localhost:7085";

let currentCartId = null;
let currentRole = null;
let authToken = null;
let currentInspectionId = null;
let currentInspectionStatus = null; // "InProgress" | "Completed"
let conferenceCarts = [];
let toastTimer = null;
let cartsById = new Map();
let cartStatusRequestId = 0;
let cartStatusReturnPage = "carts";
let pendingsRequestId = 0;
let currentPendings = [];

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
    clearCartStatus();
    clearPendings();
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
        "cart-status": "Situação do carrinho",
        conference: "Conferência",
        pendings: "Pendências",
        employees: "Funcionários"
    };
    document.getElementById("page-title").textContent = titles[page];

    if (page === "conference") loadConferencePage();
    if (page === "pendings") loadPendings();
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

    const pendingsNav = document.querySelector('.nav-item[data-page="pendings"]');
    if (pendingsNav) pendingsNav.style.display = canDoConference ? "" : "none";
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
    const canViewStatus = currentRole === "Pharmacist" || currentRole === "Nurse";

    cartsById = new Map(list.map(c => [c.id, c]));

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

            ${canViewStatus
                ? `<button class="btn" type="button" data-cart-status="${c.id}">Ver situação</button>`
                : `<button class="btn" type="button" disabled title="Somente farmacêutico e enfermeiro consultam conferências">Ver situação</button>`
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
            } else if (field.type === "textarea") {
                inputsContainer.innerHTML += `
                    ${field.label ? `<label>${field.label}</label>` : ""}
                    <textarea id="${field.id}" rows="3" placeholder="${field.placeholder ?? ""}" class="modal-input"></textarea>
                `;
            } else {
                inputsContainer.innerHTML += `
                    ${field.label ? `<label>${field.label}</label>` : ""}
                    <input type="${field.type}" id="${field.id}" placeholder="${field.placeholder ?? ""}" class="modal-input">
                `;
            }
        });

        // valor inicial e tamanho máximo entram por propriedade (não por HTML), para texto de usuário nunca virar código
        fields.forEach(field => {
            const element = document.getElementById(field.id);
            if (field.value !== undefined) element.value = field.value;
            if (field.maxLength) element.maxLength = field.maxLength;
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
        400: "Quantidade inválida. Informe um valor entre 0 e 5.",
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
    },
    replenish: {
        400: "Quantidade inválida. Informe um valor entre 1 e a quantidade pendente.",
        403: "Você não tem permissão para registrar reposições.",
        404: "Conferência ou item não encontrado. A lista foi atualizada.",
        409: "Esta pendência mudou (nova conferência ou item já reposto). A lista foi atualizada.",
        fallback: "Não foi possível registrar a reposição."
    },
    note: {
        400: "Observação inválida. Use até 200 caracteres.",
        403: "Você não tem permissão para registrar observações.",
        404: "Conferência ou item não encontrado. A lista foi atualizada.",
        409: "Esta pendência mudou (nova conferência no carrinho). A lista foi atualizada.",
        fallback: "Não foi possível salvar a observação."
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
    input.max = "5";
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

// na Situação do carrinho, mostra também quanto já foi reposto desde a conferência
function describeStatusQuantities(item) {
    const text = describeQuantities(item);
    return item.replenishedQuantity > 0 ? `${text}, repostos: ${item.replenishedQuantity}` : text;
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
    if (rawValue === "" || !Number.isInteger(quantity) || quantity < 0 || quantity > 5) {
        toast("Informe uma quantidade inteira entre 0 e 5.");
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


// ============ SITUAÇÃO DO CARRINHO (AC3) ============
async function getLastCompletedInspection(emergencyCartId) {
    try {
        const response = await requestOrThrow(
            `${API_BASE_URL}/api/inspection/last/${encodeURIComponent(emergencyCartId)}`
        );
        const data = await response.json();
        if (!data?.inspectionId || !data?.finishedAt) {
            throw new Error("Resposta inesperada ao buscar a última conferência.");
        }
        return data;
    } catch (error) {
        if (error instanceof ApiError && error.status === 404) return null; // carrinho sem conferência finalizada
        throw error;
    }
}

function formatDateTime(isoDate) {
    const date = new Date(isoDate);
    if (Number.isNaN(date.getTime())) return "data indisponível";
    return date.toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" });
}

function summarizeCartStatus(missing, surplus) {
    if (missing > 0) {
        const text = `${missing} ${missing === 1 ? "item com falta" : "itens com falta"}`;
        return surplus > 0 ? `${text} e ${surplus} com sobra` : text;
    }
    if (surplus > 0) {
        return `Sem faltas, ${surplus} ${surplus === 1 ? "item com sobra" : "itens com sobra"}`;
    }
    return "Todos os itens conferem";
}

function setCartStatusMessage(text) {
    const message = document.getElementById("cart-status-message");
    message.textContent = text;
    message.classList.toggle("hidden", !text);
}

function resetCartStatusPanel(cart) {
    document.getElementById("cart-status-title").textContent = cart ? `Carrinho ${cart.code}` : "Carrinho";
    document.getElementById("cart-status-subtitle").textContent = cart ? cart.sectorName : "";
    document.getElementById("cart-status-badge").classList.add("hidden");
    document.getElementById("cart-status-list").replaceChildren();
    document.getElementById("cart-status-footer").classList.add("hidden");
}

// descarta respostas pendentes e limpa a tela (usado no logout)
function clearCartStatus() {
    cartStatusRequestId++;
    resetCartStatusPanel(null);
    setCartStatusMessage("");
}

function buildStatusRow(item) {
    const row = document.createElement("div");
    row.className = "checklist-item";

    const info = document.createElement("div");
    const name = document.createElement("strong");
    name.textContent = item.itemName;
    const details = document.createElement("small");
    details.textContent = describeStatusQuantities(item);
    info.append(name, details);
    if (item.note) info.append(buildNoteElement(item.note));

    const found = document.createElement("span");
    found.className = "found-quantity";
    found.textContent = `Encontrado: ${item.foundQuantity ?? "—"}`;

    const badge = document.createElement("span");
    setComparisonBadge(badge, item.comparison);

    row.append(info, found, badge);
    return row;
}

function renderCartStatus(finishedAt, data) {
    const sortedItems = [...data.items].sort((a, b) => a.itemName.localeCompare(b.itemName, "pt-BR"));
    document.getElementById("cart-status-list").replaceChildren(...sortedItems.map(buildStatusRow));

    const dateBadge = document.getElementById("cart-status-badge");
    dateBadge.textContent = `Finalizada em ${formatDateTime(finishedAt)}`;
    dateBadge.classList.remove("hidden");

    const missing = data.items.filter(item => item.comparison === "Missing").length;
    const surplus = data.items.filter(item => item.comparison === "Surplus").length;
    document.getElementById("cart-status-summary").textContent = summarizeCartStatus(missing, surplus);
    document.getElementById("cart-status-footer").classList.remove("hidden");

    setCartStatusMessage("");
}

async function showCartStatus(cartId, cartInfo = null, returnPage = "carts") {
    const cart = cartInfo ?? cartsById.get(cartId);
    const requestId = ++cartStatusRequestId;
    cartStatusReturnPage = returnPage;

    showPage("cart-status");
    resetCartStatusPanel(cart);
    setCartStatusMessage("Carregando situação do carrinho...");

    try {
        const last = await getLastCompletedInspection(cartId);
        if (requestId !== cartStatusRequestId) return; // o usuário já abriu outro carrinho

        if (!last) {
            setCartStatusMessage("Este carrinho ainda não tem conferência finalizada.");
            return;
        }

        const data = await getInspectionComparison(last.inspectionId);
        if (requestId !== cartStatusRequestId) return;

        renderCartStatus(last.finishedAt, data);
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        if (requestId === cartStatusRequestId) {
            setCartStatusMessage("Não foi possível carregar a situação do carrinho. Volte e tente novamente.");
        }
    }
}

document.getElementById("cart-grid").addEventListener("click", e => {
    const button = e.target.closest("[data-cart-status]");
    if (button) showCartStatus(button.dataset.cartStatus);
});


// ============ PENDÊNCIAS (AC3) ============
async function getPendings() {
    const response = await requestOrThrow(`${API_BASE_URL}/api/inspection/pending`);
    const data = await response.json();
    if (!Array.isArray(data)) throw new Error("Resposta inesperada ao buscar as pendências.");

    // garante os campos de reposição mesmo que a API não os envie
    return data.map(pending => ({
        ...pending,
        items: pending.items.map(item => ({
            ...item,
            replenishedQuantity: item.replenishedQuantity ?? 0,
            pendingQuantity: item.pendingQuantity ?? item.missingQuantity,
            note: item.note ?? null
        }))
    }));
}

function pluralize(count, singular, plural) {
    return `${count} ${count === 1 ? singular : plural}`;
}

function setPendingsMessage(text) {
    const message = document.getElementById("pendings-message");
    message.textContent = text;
    message.classList.toggle("hidden", !text);
}

function setPendingsSummary(text) {
    const summary = document.getElementById("pendings-summary");
    summary.textContent = text;
    summary.classList.toggle("hidden", !text);
}

// descarta respostas pendentes e limpa a tela (usado no logout)
function clearPendings() {
    pendingsRequestId++;
    currentPendings = [];
    document.getElementById("pendings-list").replaceChildren();
    setPendingsMessage("");
    setPendingsSummary("");
    document.getElementById("refresh-pendings").disabled = false;
}

function buildPendingRow(item, inspectionId) {
    const row = document.createElement("div");
    row.className = "checklist-item";

    const info = document.createElement("div");
    const name = document.createElement("strong");
    name.textContent = item.itemName;
    const details = document.createElement("small");
    details.textContent = `Esperado: ${item.expectedQuantity}, encontrado: ${item.foundQuantity}`
        + (item.replenishedQuantity > 0 ? `, repostos: ${item.replenishedQuantity}` : "");
    info.append(name, details);
    if (item.note) info.append(buildNoteElement(item.note));

    const noteButton = document.createElement("button");
    noteButton.type = "button";
    noteButton.className = "text-btn note-btn";
    noteButton.textContent = item.note ? "Editar observação" : "Adicionar observação";
    noteButton.setAttribute("aria-label", `${item.note ? "Editar" : "Adicionar"} observação de ${item.itemName}`);
    noteButton.addEventListener("click", () => handleEditNote(inspectionId, item, noteButton));
    info.append(noteButton);

    const input = document.createElement("input");
    input.className = "qty";
    input.type = "number";
    input.min = "1";
    input.max = String(item.pendingQuantity);
    input.step = "1";
    input.placeholder = "Qtd";
    input.setAttribute("aria-label", `Quantidade reposta de ${item.itemName}`);

    const button = document.createElement("button");
    button.type = "button";
    button.className = "btn";
    button.textContent = "Repor";
    button.setAttribute("aria-label", `Registrar reposição de ${item.itemName}`);
    button.addEventListener("click", () => handleReplenish(inspectionId, item, input, button));
    input.addEventListener("keydown", e => {
        if (e.key === "Enter") button.click();
    });

    const group = document.createElement("div");
    group.className = "qty-group";
    group.append(input, button);

    const missing = document.createElement("span");
    missing.className = "badge badge-orange";
    missing.textContent = item.pendingQuantity === 1 ? "Falta 1" : `Faltam ${item.pendingQuantity}`;

    row.append(info, group, missing);
    return row;
}

function buildPendingCartPanel(pending) {
    const panel = document.createElement("div");
    panel.className = "panel";

    const header = document.createElement("div");
    header.className = "panel-header";
    const titleBox = document.createElement("div");
    const title = document.createElement("h3");
    title.textContent = `Carrinho ${pending.cartCode}`;
    const subtitle = document.createElement("p");
    subtitle.textContent = pending.sectorName;
    titleBox.append(title, subtitle);
    const dateBadge = document.createElement("span");
    dateBadge.className = "badge badge-blue";
    dateBadge.textContent = `Finalizada em ${formatDateTime(pending.finishedAt)}`;
    header.append(titleBox, dateBadge);

    const list = document.createElement("div");
    list.append(...pending.items.map(item => buildPendingRow(item, pending.inspectionId)));

    const footer = document.createElement("div");
    footer.className = "conference-footer";
    const total = document.createElement("strong");
    total.textContent = pluralize(pending.items.length, "item em falta", "itens em falta");
    const statusButton = document.createElement("button");
    statusButton.type = "button";
    statusButton.className = "btn";
    statusButton.textContent = "Ver situação";
    statusButton.setAttribute("aria-label", `Ver situação do carrinho ${pending.cartCode}`);
    statusButton.addEventListener("click", () =>
        showCartStatus(pending.emergencyCartId, { code: pending.cartCode, sectorName: pending.sectorName }, "pendings"));
    footer.append(total, statusButton);

    panel.append(header, list, footer);
    return panel;
}

function renderPendings(pendings) {
    currentPendings = pendings;
    document.getElementById("pendings-list").replaceChildren(...pendings.map(buildPendingCartPanel));

    if (pendings.length === 0) {
        setPendingsSummary("");
        setPendingsMessage("Nenhum item em falta nas últimas conferências finalizadas.");
        return;
    }

    const totalItems = pendings.reduce((sum, pending) => sum + pending.items.length, 0);
    setPendingsMessage("");
    setPendingsSummary(
        `${pluralize(totalItems, "item em falta", "itens em falta")} em ${pluralize(pendings.length, "carrinho", "carrinhos")}`
    );
}

async function loadPendings() {
    const requestId = ++pendingsRequestId;
    const refreshButton = document.getElementById("refresh-pendings");

    refreshButton.disabled = true;
    document.getElementById("pendings-list").replaceChildren();
    setPendingsSummary("");
    setPendingsMessage("Carregando pendências...");

    try {
        const pendings = await getPendings();
        if (requestId !== pendingsRequestId) return; // chegou uma carga mais nova

        renderPendings(pendings);
    } catch (error) {
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        if (requestId === pendingsRequestId) {
            setPendingsMessage("Não foi possível carregar as pendências. Clique em Atualizar para tentar de novo.");
        }
    } finally {
        if (requestId === pendingsRequestId) refreshButton.disabled = false;
    }
}

async function registerReplenishment(inspectionId, itemId, quantity) {
    const response = await requestOrThrow(
        `${API_BASE_URL}/api/inspection/${encodeURIComponent(inspectionId)}/items/${encodeURIComponent(itemId)}/replenishments`,
        {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ quantity })
        }
    );
    const data = await response.json();
    if (typeof data?.pendingQuantity !== "number") {
        throw new Error("Resposta inesperada ao registrar a reposição.");
    }
    return data;
}

// atualiza a lista na tela com o que a API confirmou, sem recarregar tudo
function applyReplenishment(inspectionId, itemId, pendingQuantity) {
    const pending = currentPendings.find(p => p.inspectionId === inspectionId);
    const item = pending?.items.find(i => i.itemId === itemId);
    if (item) {
        item.pendingQuantity = pendingQuantity;
        item.replenishedQuantity = item.missingQuantity - pendingQuantity;
    }

    renderPendings(
        currentPendings
            .map(p => ({ ...p, items: p.items.filter(i => i.pendingQuantity > 0) }))
            .filter(p => p.items.length > 0)
    );
}

async function handleReplenish(inspectionId, item, input, button) {
    if (button.disabled) return; // já há um envio em andamento para esta linha

    const rawValue = input.value.trim();
    const quantity = Number(rawValue);
    if (rawValue === "" || !Number.isInteger(quantity) || quantity < 1 || quantity > item.pendingQuantity) {
        toast(`Informe uma quantidade inteira entre 1 e ${item.pendingQuantity}.`);
        return;
    }

    button.disabled = true;
    input.disabled = true;

    try {
        const result = await registerReplenishment(inspectionId, item.itemId, quantity);
        applyReplenishment(inspectionId, item.itemId, result.pendingQuantity);

        toast(result.pendingQuantity === 0
            ? "Pendência resolvida."
            : `Reposição registrada. Ainda ${result.pendingQuantity === 1 ? "falta 1" : `faltam ${result.pendingQuantity}`}.`);
    } catch (error) {
        button.disabled = false;
        input.disabled = false;
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast(conferenceErrorMessage("replenish", error));

        // a pendência mudou no servidor: recarrega para a tela não ficar desatualizada
        if (error instanceof ApiError && (error.status === 404 || error.status === 409)) loadPendings();
    }
}

function buildNoteElement(note) {
    const element = document.createElement("small");
    element.className = "item-note";
    element.textContent = `Obs.: ${note}`;
    return element;
}

async function registerNote(inspectionId, itemId, note) {
    await requestOrThrow(
        `${API_BASE_URL}/api/inspection/${encodeURIComponent(inspectionId)}/items/${encodeURIComponent(itemId)}/note`,
        {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ note })
        }
    );
}

function applyNote(inspectionId, itemId, note) {
    const pending = currentPendings.find(p => p.inspectionId === inspectionId);
    const item = pending?.items.find(i => i.itemId === itemId);
    if (item) item.note = note;

    renderPendings(currentPendings);
}

async function handleEditNote(inspectionId, item, button) {
    if (button.disabled) return;

    const result = await openModal(`Observação: ${item.itemName}`, [
        {
            id: "itemNote",
            type: "textarea",
            label: "Observação (opcional, até 200 caracteres)",
            placeholder: "Ex.: reposição prevista para dezembro",
            value: item.note ?? "",
            maxLength: 200
        }
    ]);
    if (!result) return; // cancelou

    // uma linha só: espaços e quebras de linha repetidos viram um espaço
    const note = result.itemNote.replace(/\s+/g, " ").trim();
    if (note === (item.note ?? "")) return; // nada mudou

    button.disabled = true;
    try {
        await registerNote(inspectionId, item.itemId, note);
        applyNote(inspectionId, item.itemId, note === "" ? null : note);
        toast(note === "" ? "Observação removida." : "Observação salva.");
    } catch (error) {
        button.disabled = false;
        if (error instanceof SessionExpiredError) return;
        console.error(error);
        toast(conferenceErrorMessage("note", error));

        // a pendência mudou no servidor: recarrega para a tela não ficar desatualizada
        if (error instanceof ApiError && (error.status === 404 || error.status === 409)) loadPendings();
    }
}

document.getElementById("refresh-pendings").addEventListener("click", loadPendings);
document.getElementById("cart-status-back").addEventListener("click", () => showPage(cartStatusReturnPage));
