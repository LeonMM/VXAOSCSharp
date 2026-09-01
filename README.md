# VXAOSCSharp

Adaptação do Servidor Ruby de VXA-OS para C#, e mais além.

Projeto Original:  https://github.com/Valentine90/vxa-os

A versão usada aqui é a versão 2.1.6 com as minhas modificações (Estados e Cooldown).

## Planejamento de Desenvolvimento
- [ ] Primeiro: Adaptar o Servidor Ruby do VXA-OS para C#;
- [ ] Segundo: Criar um novo Cliente em C#, ainda decidindo entre Godot ou Monogame ou outro motor ou API;
- [ ] Terceiro: Adaptar o novo Servidor e o novo Cliente do VXA-OS para ler arquivos do RPG Maker MZ;
- [ ] Terceiro.1: Renomear projeto para MZC#OS ou algum novo nome, aberto a sugestões;
- [ ] Quarto: Melhorar o Servidor para aceitar plugins sem necessidade de editar código fonte e compila-lo a cada modificação.

## Motivação e Meta

Sim, a meta é aos poucos trocar de VXA para RPG Maker MZ, pois é um editor mais versátil que o VXAce. Mas não só isso,
MZ exporta os dados em JSON, o que torna facil a leitura pelo Servidor e por um novo Cliente.

Ruby tem gerado muitos transtornos, e particularmente estou cansado dos problemas de desempenho.

Quando entrar no Segundo passo, irei desenvolver uma ferramenta para converter projetos de RPG Maker VX Ace para MZ.

Script para fazer isso já existe, mas apenas para MV, e nos meus testes acabou dando uma certa incompatibilidade.

VXA-OS continua sendo uma ótima ferramenta, para quem quer fazer um protótipo ou projeto pequeno.
Mas para quem quer fazer algo maior, os problemas acabam atrapalhando, então esse, vocês que estão almejando o grande
são a principal motivação que tenho para fazer este projeto, uma versão mais estavel, visando o grande.

## Avisos

Não há previsão de quando irei concluir quaisquer um dos passos, pelo menos Servidor quero terminar o mais cedo possivel.

O Servidor C# já pode acaber resolvendo boa parte dos lags e delays que ocorrem no VXA-OS atualmente.

Não estou abandonando VXA-OS, apenas evoluindo seu conceito e subindo o nível.

Aqui é tecnicamente uma fork do VXA-OS, onde essa versão do projeto vai seguir um caminho diferente da versão do Valentine.

## 📦 Pacotes NuGet

O projeto utiliza os seguintes pacotes NuGet:

| Pacote                                                                                                             |    Versão | Descrição                                                |
| ------------------------------------------------------------------------------------------------------------------ | --------: | -------------------------------------------------------- |
| [Dapper](https://www.nuget.org/packages/Dapper/)                                                                   |  `2.1.79` | Micro-ORM para acesso ao banco de dados.                 |
| [Microsoft.CodeAnalysis.CSharp](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp/)                     |   `5.6.0` | APIs do compilador C# (Roslyn).                          |
| [Microsoft.CodeAnalysis.CSharp.Scripting](https://www.nuget.org/packages/Microsoft.CodeAnalysis.CSharp.Scripting/) |   `5.6.0` | Suporte à execução de scripts C# em tempo de execução.   |
| [Microsoft.Data.Sqlite](https://www.nuget.org/packages/Microsoft.Data.Sqlite/)                                     | `10.0.10` | Provedor de acesso ao SQLite.                            |
| [MySqlConnector](https://www.nuget.org/packages/MySqlConnector/)                                                   |   `2.6.1` | Driver .NET para MySQL e MariaDB.                        |
| [NCalc](https://www.nuget.org/packages/NCalc/)                                                                     |   `7.1.0` | Avaliação e interpretação de expressões matemáticas.     |
| [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)                                                 |  `13.0.4` | Serialização e desserialização de JSON.                  |
| [Npgsql](https://www.nuget.org/packages/Npgsql/)                                                                   |  `10.0.3` | Provedor de acesso ao PostgreSQL.                        |
| [SQLitePCLRaw.lib.e_sqlite3](https://www.nuget.org/packages/SQLitePCLRaw.lib.e_sqlite3/)                           |  `2.1.12` | Biblioteca nativa SQLite utilizada pelo provedor SQLite. |
| [SqlKata](https://www.nuget.org/packages/SqlKata/)                                                                 |   `4.0.1` | Query Builder para construção de consultas SQL.          |
| [SqlKata.Execution](https://www.nuget.org/packages/SqlKata.Execution/)                                             |   `4.0.1` | Execução de consultas SQL utilizando SqlKata.            |

# 🚀 Tutorial de Uso

## 1. Abrindo o projeto

Para abrir e compilar o projeto, utilize o **Visual Studio**.

O projeto foi desenvolvido utilizando o **Visual Studio Community 2022**.

---

## 2. Requisitos

O projeto tem como alvo **.NET 9 para Windows 64-bit (Win64)**.

Também é possível compilar o projeto para **Linux**, desde que o ambiente possua o **.NET 9 SDK** instalado.

Para instalar o .NET 9 SDK, consulte a documentação oficial da Microsoft:

https://dotnet.microsoft.com/download/dotnet/9.0

### Gerando para Linux

Gerar executável ÚNICO para Linux (Sem precisar instalar .NET no servidor Linux):

```bash
dotnet publish -c Release -r linux-x64 --self-contained true
```

Gerar executável para Linux (Dependendo do .NET 9 instalado no servidor):

```bash
dotnet publish -c Release -r linux-x64 --self-contained false -p:PublishSingleFile=true
```

---

## 3. Testando o servidor

Durante o desenvolvimento e os testes, recomenda-se executar o servidor no modo **Debug**.

Isso permite analisar com maior facilidade as mensagens de erro e identificar possíveis problemas durante a execução.

---

## 4. Executando o servidor fora do Debug

Para utilizar o servidor fora do Visual Studio, compile o projeto em **Release**.

Depois, copie o conteúdo da seguinte pasta:

```text
bin/Release/net9.0/(plataforma)
```

para uma pasta de sua escolha.

Essa pasta será considerada a **pasta do Servidor**.

Por exemplo:

```text
Servidor/
├── VXAOS_Server.exe
├── server.cfg
├── Data/
└── ...
```

A estrutura final pode variar de acordo com a plataforma e a configuração utilizada na publicação.

---

## 5. Pasta Data

Caso não exista uma pasta `Data`, tanto durante a execução em **Debug** quanto em **Release**, crie manualmente uma:

```text
Data/
```

Depois, copie do servidor original do **VXA-OS** os seguintes arquivos:

```text
Database.db
switches.json
```

A estrutura deverá ficar semelhante a:

```text
Servidor/
├── Data/
│   ├── Database.db
│   └── switches.json
├── server.cfg
└── ...
```

---

## 6. Configurando o `server.cfg`

O arquivo `server.cfg` fica na raiz da pasta do servidor.

Nele é possível configurar o caminho da pasta `Data` e o banco de dados utilizado pelo servidor.

### DATA_PATH

Altere `DATA_PATH` para o caminho real da pasta `Data` do cliente a ser utilizada pelo servidor.

Exemplo:

```ini
DATA_PATH=C:\VXAOS\Client\Data
```

No Linux, por exemplo:

```ini
DATA_PATH=/home/vxaos/Client/Data
```

### Configuração do banco de dados

O tipo de banco de dados pode ser alterado através de `DB_TYPE`.

Utilize um dos valores disponíveis no arquivo de configuração:

```ini
DB_TYPE=0
```

Os valores correspondem aos diferentes tipos de banco de dados suportados pelo servidor.

Para **PostgreSQL** ou **MySQL**, configure também os demais parâmetros `DB_*`, como host, porta, usuário, banco de dados, etc.

Exemplo:

```ini
DB_TYPE=0
DB_HOST=127.0.0.1
DB_PORT=5432
DB_USER=postgres
DB_NAME=VXAOS
```

Para utilizar **SQLite**, basta alterar `DB_FILE` para o nome do arquivo `.db` localizado dentro da pasta `Data`.

Exemplo:

```ini
DB_TYPE=2
DB_FILE=Database.db
```

---

## 7. Requisito do Ruby

É necessário possuir pelo menos o **Ruby 3.x** instalado no servidor.

Além do Ruby, o servidor utiliza apenas uma Gem adicional para inicializar:

```text
base64
```

Instale-a utilizando:

```bash
gem install base64
```

Depois disso, o servidor deverá possuir todos os requisitos necessários para sua inicialização.

---

## 8. Cliente recomendado

Como este projeto ainda está em fase **experimental**, recomenda-se utilizar **apenas o Cliente fornecido junto ao projeto para realizar os testes**.

O cliente e o servidor ainda podem possuir incompatibilidades e bugs.

Assim que os problemas forem corrigidos, este tutorial será atualizado com as modificações necessárias no Cliente.

---

## 9. Documentação do código

Em breve serão adicionados comentários em todo o código do servidor, com explicações sobre seu funcionamento e sobre as principais partes da implementação.

Isso facilitará a compreensão e a utilização do projeto por outros desenvolvedores.


## Notas Finais

Agradeço ao Valentine por criar o VXA-OS.

Vou atualizando isso com informações mais técnicas no futuro, tal qual packets usados, e créditos a terceiros envolvidos direta ou indiretamente.
