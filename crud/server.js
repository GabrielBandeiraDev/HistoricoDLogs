const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');
const path = require('path');

const app = express();
const PORT = 3000;

// Middleware para parsear JSON
app.use(bodyParser.json());

// Rota para receber logs
app.post('/api/logs', (req, res) => {
    const logs = req.body;

    const filePath = path.join(__dirname, 'logs.json');

    fs.readFile(filePath, 'utf8', (err, data) => {
        if (err) {
            // Cria o arquivo se não existir
            fs.writeFile(filePath, JSON.stringify([], null, 2), (err) => {
                if (err) {
                    console.error('Erro ao criar o arquivo de logs:', err);
                    return res.status(500).send('Erro ao criar o arquivo de logs.');
                }
                return res.status(201).send('Arquivo criado, mas sem logs.');
            });
        } else {
            try {
                const existingLogs = JSON.parse(data || '[]');
                existingLogs.push(...logs); // Adiciona os novos logs

                fs.writeFile(filePath, JSON.stringify(existingLogs, null, 2), (err) => {
                    if (err) {
                        console.error('Erro ao salvar os logs:', err);
                        return res.status(500).send('Erro ao salvar os logs.');
                    }
                    res.status(201).send('Logs recebidos com sucesso.');
                });
            } catch (parseErr) {
                console.error('Erro ao parsear JSON:', parseErr);
                return res.status(400).send('Formato de JSON inválido.');
            }
        }
    });
});

// Rota para carregar logs
app.get('/api/logs', (req, res) => {
    const filePath = path.join(__dirname, 'logs.json');

    fs.readFile(filePath, 'utf8', (err, data) => {
        if (err) {
            console.error('Erro ao ler o arquivo de logs:', err);
            return res.status(500).send('Erro ao ler o arquivo de logs.');
        }

        const logs = JSON.parse(data || '[]');
        res.json(logs);
    });
});

// Iniciar o servidor
app.listen(PORT, () => {
    console.log(`Servidor rodando na porta ${PORT}`);
});