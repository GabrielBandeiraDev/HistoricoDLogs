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
    let aprovados = 0;
    let reprovados = 0;

    // Agrupa os logs em conjuntos de até 8 e conta cada grupo
    for (let i = 0; i < allLogs.length; i += 8) {
        const grupo = allLogs.slice(i, i + 8);

        // Verifica se algum log do grupo tem o resultado "Reprovado"
        const algumReprovado = grupo.some(log => log.Resultado.toUpperCase() == 'REPROVADO');

        // Incrementa a contagem de grupos aprovados ou reprovados
        if (algumReprovado) {
            reprovados++;
        } else {
            aprovados++;
        }
    }

    // Atualiza os elementos HTML com as contagens
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
    
    // Limpa o conteúdo anterior das seções de aprovados e reprovados
    historicoAprovadosDiv.innerHTML = '';
    historicoReprovadosDiv.innerHTML = '';

    // Divide os logs em grupos de até 8
    const gruposDeLogs = [];
    for (let i = 0; i < allLogs.length; i += 8) {
        gruposDeLogs.push(allLogs.slice(i, i + 8));
    }

    // Cria uma log-box para cada grupo de até 8 logs
    gruposDeLogs.forEach((grupo, index) => {
        // Verifica se algum log no grupo está reprovado
        const algumReprovado = grupo.some(log => log.Resultado === 'Reprovado');
        
        // Cria a log-box
        const logBox = document.createElement('div');
        logBox.className = algumReprovado ? 'log-box log-box-2' : 'log-box log-box-1';
        
        // Define o status geral do grupo (Aprovado ou Reprovado)
        const statusGrupo = algumReprovado ? 'Reprovado' : 'Aprovado';
        
        // Usa o primeiro log do grupo como resumo para exibir as informações adicionais
        const primeiroLog = grupo[0];
        
        logBox.innerHTML = `
            <strong>ID Do Teste: ${index + 1}</strong><br>
            <strong>Status: ${statusGrupo}</strong><br>
            <strong>Data/Hora:</strong> ${new Date(primeiroLog.DataHora).toLocaleString()}<br>
            <strong>Modelo:</strong> ${primeiroLog.Modelo}<br>
            <strong>Serial:</strong> ${primeiroLog.DadosSerial || 'Não disponível'}<br>
        `;
    
        // Adiciona o evento de clique para abrir o modal com detalhes
        logBox.addEventListener('click', () => {
            abrirModalDetalhes(grupo);
        });
    
        // Adiciona a log-box na seção correta (aprovados ou reprovados)
        if (algumReprovado) {
            historicoReprovadosDiv.appendChild(logBox);
        } else {
            historicoAprovadosDiv.appendChild(logBox);
        }
    });
}

// Função para abrir o modal com os detalhes dos logs do grupo
function abrirModalDetalhes(grupoDeLogs) {
    const logDetails = document.getElementById('logDetails');
    logDetails.innerHTML = ''; // Limpa conteúdo anterior

    // Adiciona os logs do grupo ao modal
    grupoDeLogs.forEach(log => {
        const logDetail = document.createElement('div');
        logDetail.className = 'log-detail';
        logDetail.innerHTML = `
            <strong>ID:</strong> ${log.Id}<br>
            <strong>Nome:</strong> ${log.Nome}<br>
            <strong>Resultado:</strong> ${log.Resultado}<br>
            <strong>Data e Hora:</strong> ${new Date(log.DataHora).toLocaleString()}<br>
            <strong>Modelo:</strong> ${log.Modelo}<br>
            <strong>Dados da Serial:</strong> ${log.DadosSerial || 'Não disponível'}<br>
        `;
        logDetails.appendChild(logDetail);
    });

    // Exibe o modal
    document.getElementById('logModal').style.display = 'block';
}



// Fechar o modal ao clicar no botão de fechar
document.getElementById('closeModal').onclick = function() {
    document.getElementById('logModal').style.display = 'none';
}

// Fechar o modal ao clicar fora do conteúdo
window.onclick = function(event) {
    if (event.target == document.getElementById('logModal')) {
        document.getElementById('logModal').style.display = 'none';
    }
}


// Fechar o modal ao clicar no botão de fechar
document.getElementById('closeModal').onclick = function() {
    document.getElementById('logModal').style.display = 'none';
}

// Fechar o modal ao clicar fora do conteúdo
window.onclick = function(event) {
    if (event.target == document.getElementById('logModal')) {
        document.getElementById('logModal').style.display = 'none';
    }
}


// Fechar o modal ao clicar no botão de fechar
document.getElementById('closeModal').onclick = function() {
    document.getElementById('logModal').style.display = 'none';
}

// Fechar o modal ao clicar fora do conteúdo
window.onclick = function(event) {
    if (event.target == document.getElementById('logModal')) {
        document.getElementById('logModal').style.display = 'none';
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

            const filtroData = document.getElementById('filterData').value; // Data selecionada pelo usuário
            const dataSelecionada = filtroData ? new Date(filtroData).toISOString().split('T')[0] : new Date().toISOString().split('T')[0];
            
            // Filtra apenas os logs do dia selecionado
            const logsDoDia = data.filter(log => new Date(log.DataHora).toISOString().split('T')[0] === dataSelecionada);

            if (logsDoDia.length === 0) {
                allLogs = [];
                atualizarContagem();
                atualizarGrafico(0, 0);
                document.getElementById('ultimoRelatorioContent').innerHTML = `Nenhum relatório disponível para ${dataSelecionada}.`;
                return;
            }

            const newTimestamp = new Date(logsDoDia[logsDoDia.length - 1].DataHora).toISOString();
            if (lastUpdateTimestamp !== newTimestamp) {
                allLogs = logsDoDia;
                mostrarHistorico();
                atualizarContagem();

                const aprovados = allLogs.filter(log => log.Resultado === 'Aprovado').length;
                const reprovados = allLogs.filter(log => log.Resultado === 'Reprovado').length;
                atualizarGrafico(aprovados, reprovados);

                lastUpdateTimestamp = newTimestamp;

                const logsUltimoRelatorio = logsDoDia.filter(log => new Date(log.DataHora).toISOString() === lastUpdateTimestamp);
                const ultimoRelatorioContent = logsUltimoRelatorio.map(log => 
                    `<strong>ID:</strong> ${log.Id}<br>
                    <strong>Nome:</strong> ${log.Nome}<br>
                    <strong>Resultado:</strong> ${log.Resultado}<br>
                    <strong>Data e Hora:</strong> ${new Date(log.DataHora).toLocaleString()}`
                ).join('');

                document.getElementById('ultimoRelatorioContent').innerHTML = ultimoRelatorioContent || `Nenhum relatório disponível para ${dataSelecionada}.`;
            }
        })
        .catch(err => {
            console.error('Erro ao carregar o histórico:', err);
        });
}


document.getElementById('loadHistorico').onclick = () => {
    const filtroData = document.getElementById('filterData').value || new Date().toISOString().split('T')[0];
    const url = `/api/download-pdf?data=${filtroData}`; // Certifique-se de que a URL corresponda à sua API

    // Faz uma requisição para o backend para gerar e baixar o PDF
    fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error('Erro ao gerar o PDF.');
            }
            return response.blob(); // Obtém o arquivo PDF como um Blob
        })
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `historico_logs_${filtroData}.pdf`; // Nome do arquivo PDF
            document.body.appendChild(a);
            a.click(); // Simula o clique no link para iniciar o download
            a.remove(); // Remove o link do DOM
        })
        .catch(err => {
            console.error('Erro ao baixar o PDF:', err);
        });
};





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
