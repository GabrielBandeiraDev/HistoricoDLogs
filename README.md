# Sistema de Gestão de Logs

## 🖥️ Introdução
Bem-vindo ao Sistema de Gestão de Logs! Este sistema foi desenvolvido para oferecer uma interface web intuitiva e eficiente, destinada à visualização, análise e gerenciamento de logs gerados por um servidor Express. A ferramenta permite ao usuário acessar e manipular logs em formato JSON de maneira simples e rápida.

## 📑 Índice
- Recursos
- Tecnologias Utilizadas
- Instalação
- Configuração do Servidor
- Uso da Aplicação
- Estrutura dos Dados
- Contribuição
- Licença
- Contato
- Imagem do Sistema

## 🚀 Recursos
- Visualização de Logs: Interface amigável para exibir logs em tempo real.
- Filtragem e Pesquisa: Busca eficiente por data, severidade e palavras-chave nos logs.
- Exportação de Dados: Logs podem ser exportados para CSV ou JSON para análises externas.
- Responsividade: Design adaptável para dispositivos móveis e desktops, garantindo acessibilidade em diferentes plataformas.

## 🛠️ Tecnologias Utilizadas
### Frontend:
- HTML, CSS e JavaScript
- Bootstrap

### Backend:
- Node.js e Express
- Middleware para manipulação de JSON

### Banco de Dados:
- MongoDB (ou outro banco de dados de sua escolha)

## ⚙️ Instalação
Clone o repositório:

```bash
git clone https://github.com/gabrielbandeiradev/sistema-gestao-logs.git


## Navegue até o diretório:

cd sistema-gestao-logs

## Instale as dependências:

npm install

🖧 Configuração do Servidor
Configure as variáveis de ambiente no arquivo .env (exemplo fornecido no .env.example).

Execute o servidor:

node server.js

📝 Uso da Aplicação
Após iniciar o servidor, a interface web estará disponível no navegador no endereço configurado (por padrão: http://localhost:3000).

Utilize as opções de busca e filtragem para navegar pelos logs.
Exporte logs para análise externa utilizando os botões de exportação.

📂 Estrutura dos Dados

Os logs são estruturados em JSON com os seguintes campos principais:

timestamp: Data e hora do log.
severity: Nível de severidade (Ex: INFO, ERROR, WARN).
message: Mensagem descritiva do evento.
metadata: Informações adicionais relacionadas ao evento (opcional).

🤝 Contribuição
Contribuições são bem-vindas! Siga os seguintes passos para contribuir:

Fork o projeto.

Crie uma nova branch:

git checkout -b minha-feature


Faça suas alterações e commit:

git commit -m "Adicionei uma nova feature"

Envie suas alterações:

git push origin minha-feature


📄 Licença
Este projeto está sob a licença MIT. Consulte o arquivo LICENSE para mais detalhes.

📧 Contato
Para dúvidas ou sugestões, entre em contato:

Email: gabriel_bandeira2013@hotmail.com
GitHub: GabrielbandeiraDev

