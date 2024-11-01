let allLogs = [];
let autoRefreshInterval;
let lastUpdateTimestamp = null; // Para armazenar o timestamp do último log recebido
let pieChart; // Variável para armazenar o gráfico

function startAutoRefresh() {
    autoRefreshInterval = setInterval(carregarHistorico, 5000);
}

function stopAutoRefresh() {
    clearInterval(autoRefreshInterval);
}

function atualizarContagem() {
    const aprovados = allLogs.filter(log => log.Resultado === 'Aprovado').length;
    const reprovados = allLogs.filter(log => log.Resultado === 'Reprovado').length;

    document.getElementById('approvedCount').textContent = `${aprovados}`;
    document.getElementById('rejectedCount').textContent = `${reprovados}`;
}





function atualizarGrafico(aprovados, reprovados) {
    const ctx = document.getElementById('pieChart').getContext('2d');

    if (pieChart) {
        pieChart.destroy(); // Destroi o gráfico anterior se já existir
    }

    pieChart = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ['Aprovados', 'Reprovados'],
            datasets: [{
                label: 'Resultados',
                data: [aprovados, reprovados],
                backgroundColor: ['#28a745', '#dc3545'],
                borderColor: ['#28a745', '#dc3545'],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'top',
                }
            }
        }
    });
}

// Exibe a área de login ao carregar a página
document.getElementById('loginContainer').style.display = 'block';
document.getElementById('overlay').style.display = 'block';

// Função para fechar a área de login
document.getElementById('closeLogin').onclick = function() {
    document.getElementById('loginContainer').style.display = 'none';
    document.getElementById('overlay').style.display = 'none';
};

// Função de login
document.getElementById('loginForm').onsubmit = function(e) {
    e.preventDefault();
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    // Verificação simples de usuário e senha (substitua por sua lógica)
    if (username === 'admin' && password === 'senha123') {
        document.getElementById('loginContainer').style.display = 'none';
        document.getElementById('overlay').style.display = 'none';
        document.getElementById('historicoContainer').style.display = 'block'; // Exibir histórico
        carregarHistorico(); // Carregar histórico após o login
        startAutoRefresh(); // Iniciar atualização automática
    } else {
        alert('Usuário ou senha incorretos.');
    }
};

function mostrarHistorico() {
    const historicoAprovadosDiv = document.getElementById('historico-aprovados');
    const historicoReprovadosDiv = document.getElementById('historico-reprovados');
    const filtroResultado = document.getElementById('filterResultado').value;
    const filtroModelo = parseInt(document.getElementById('filterModelo').value, 10); // Converte para número
    const filtroData = document.getElementById('filterData').value; // Captura a data do filtro

    // Limpar as divs de histórico
    historicoAprovadosDiv.innerHTML = '';
    historicoReprovadosDiv.innerHTML = '';

    // Verificar se há logs que correspondem aos filtros
    let logsEncontrados = false;

    // Formatar a data do filtro, se fornecida
    let filtroDataFormatada;
    if (filtroData) {
        filtroDataFormatada = new Date(filtroData).toISOString().split('T')[0]; // Formato YYYY-MM-DD
    }

    // Filtrar logs de acordo com os critérios
    const logsFiltrados = allLogs.filter(log => {
        const logDate = new Date(log.DataHora).toISOString().split('T')[0]; // Formato YYYY-MM-DD
        const isDateMatch = !filtroData || logDate === filtroDataFormatada; // Verifica se a data do log corresponde ao filtro
        const isModeloMatch = isNaN(filtroModelo) || log.Modelo === filtroModelo; // Verifica se o modelo corresponde ao filtro
        const isResultadoMatch = !filtroResultado || log.Resultado === filtroResultado; // Verifica se o resultado corresponde ao filtro

        return isDateMatch && isModeloMatch && isResultadoMatch; // Retorna verdadeiro se atender todos os filtros
    });

    // Atualizar contagens e exibir logs filtrados
    const aprovados = logsFiltrados.filter(log => log.Resultado === 'Aprovado').length;
    const reprovados = logsFiltrados.filter(log => log.Resultado === 'Reprovado').length;

    // Atualizar contagens na interface
    document.getElementById('approvedCount').textContent = `${aprovados}`;
    document.getElementById('rejectedCount').textContent = `${reprovados}`;

    // Atualizar gráfico com os logs filtrados
    atualizarGrafico(aprovados, reprovados);

    logsFiltrados.forEach((log) => {
        const logElement = document.createElement('div');
        logElement.className = log.Resultado === 'Aprovado' ? 'log-box log-box-1' : 'log-box log-box-2';
        logElement.innerHTML = `<strong>ID:</strong> ${log.Id}<br>
                                <strong>Nome:</strong> ${log.Nome}<br>
                                <strong>Resultado:</strong> ${log.Resultado}<br>
                                <strong>Data e Hora:</strong> ${new Date(log.DataHora).toLocaleString()}<br>
                                <strong>Modelo:</strong> ${log.Modelo}`;

        // Adicionar o log no contêiner correto com base no resultado
        if (log.Resultado === 'Aprovado') {
            historicoAprovadosDiv.appendChild(logElement);
        } else if (log.Resultado === 'Reprovado') {
            historicoReprovadosDiv.appendChild(logElement);
        }
    });

    // Se nenhum log foi encontrado, mostrar uma mensagem
    if (logsFiltrados.length === 0) {
        const noResultsMessage = document.createElement('p');
        noResultsMessage.textContent = 'Nenhum log encontrado para os filtros aplicados.';
        historicoAprovadosDiv.appendChild(noResultsMessage);
        historicoReprovadosDiv.appendChild(noResultsMessage.cloneNode(true));
    }
}




document.getElementById('filterButton').onclick = () => {
    mostrarHistorico();
};

function carregarHistorico() {
    fetch('/api/logs')
        .then(response => response.json())
        .then(data => {
            if (data.length === 0) {
                allLogs = [];
                atualizarContagem();
                atualizarGrafico(0, 0);
                document.getElementById('ultimoRelatorioContent').innerHTML = 'Nenhum relatório disponível.';
                return;
            }

            const currentDate = new Date().toISOString().split('T')[0];
            const logsHoje = data.filter(log => new Date(log.DataHora).toISOString().split('T')[0] === currentDate);

            if (logsHoje.length === 0) {
                allLogs = [];
                atualizarContagem();
                atualizarGrafico(0, 0);
                document.getElementById('ultimoRelatorioContent').innerHTML = 'Nenhum relatório disponível para hoje.';
                return;
            }

            const newTimestamp = new Date(logsHoje[logsHoje.length - 1].DataHora).toISOString();
            if (lastUpdateTimestamp !== newTimestamp) {
                allLogs = data;
                mostrarHistorico();
                atualizarContagem();

                const aprovados = allLogs.filter(log => log.Resultado === 'Aprovado').length;
                const reprovados = allLogs.filter(log => log.Resultado === 'Reprovado').length;
                atualizarGrafico(aprovados, reprovados);

                lastUpdateTimestamp = newTimestamp;

                const logsUltimoRelatorio = logsHoje.filter(log => new Date(log.DataHora).toISOString() === lastUpdateTimestamp);
                const ultimoRelatorioContent = logsUltimoRelatorio.map(log => 
                    `<strong>ID:</strong> ${log.Id}<br>
                    <strong>Nome:</strong> ${log.Nome}<br>
                    <strong>Resultado:</strong> ${log.Resultado}<br>
                    <strong>Data e Hora:</strong> ${new Date(log.DataHora).toLocaleString()}`
                ).join('');

                document.getElementById('ultimoRelatorioContent').innerHTML = ultimoRelatorioContent || 'Nenhum relatório disponível.';
            }
        })
        .catch(err => {
            console.error('Erro ao carregar o histórico:', err);
        });
}

document.getElementById('loadHistorico').onclick = () => {
    carregarHistorico();
};

document.getElementById('refreshButton').onclick = () => {
    carregarHistorico();
};

function addNotification(message, type = 'info') {
    const notificationList = document.getElementById('notificationList');
    const notificationItem = document.createElement('li');
    notificationItem.textContent = message;
    notificationItem.classList.add(type === 'success' ? 'success' : 'error');

    notificationList.appendChild(notificationItem);

    setTimeout(() => {
        notificationItem.style.opacity = 0;
        setTimeout(() => notificationItem.remove(), 500);
    }, 5000);
}
