const API_BASE_URL = "https://localhost:7085";

let currentCartId = null;
let currentRole = null;
let authToken = null;

class SessionExpiredError extends Error {}

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

function renderChecklist() {
    document.getElementById("checklist").innerHTML = items.map((item, i) => `
        <div class="checklist-item">
            <div>
                <strong>${item.name}</strong>
                <small>${item.category} · Necessário: ${item.required}</small>
            </div>
            <input class="qty" type="number" min="0" value="${item.actual}" data-index="${i}">
            <span class="badge ${item.actual >= item.required ? "badge-green" : "badge-orange"}">
                ${item.actual >= item.required ? "OK" : "Pendente"}
            </span>
        </div>
    `).join("");
    updatePending();
}

function updatePending() {
    let pending = 0;
    document.querySelectorAll(".qty").forEach(input => {
        const item = items[Number(input.dataset.index)];
        if (Number(input.value) < item.required) pending++;
    });
    document.getElementById("pending-count").textContent =
        `${pending} ${pending === 1 ? "item pendente" : "itens pendentes"}`;
}

function toast(message) {
    const el = document.getElementById("toast");
    el.textContent = message;
    el.classList.add("show");
    setTimeout(() => el.classList.remove("show"), 2600);
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

document.getElementById("checklist").addEventListener("input", updatePending);

document.getElementById("finish-conference").addEventListener("click", () => {
    const pending = [...document.querySelectorAll(".qty")]
        .filter(input => Number(input.value) < items[Number(input.dataset.index)].required).length;

    toast(pending ? `Conferência finalizada com ${pending} pendências.` : "Conferência finalizada com sucesso!");
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
