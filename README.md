# VXAOSCSharp

Adaptação do Servidor Ruby de VXA-OS para C#, e mais além.

Projeto Original:  https://github.com/Valentine90/vxa-os

A versão usada aqui é a versão 2.1.6 com as minhas modificações (Estados e Cooldown).

## Planejamento de Desenvolvimento
- [ ] Primeiro: Adaptar o Servidor Ruby do VXA-OS para C#;
- [ ] Segundo: Adaptar o novo Servidor e o Cliente do VXA-OS para ler arquivos do RPG Maker MZ;
- [ ] Terceiro: Criar um novo Cliente em C#, ainda decidindo entre Godot ou Monogame ou outro motor ou API;
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

## Notas Finais

Agradeço ao Valentine por criar o VXA-OS.

Vou atualizando isso com informações mais técnicas no futuro, tal qual packets usados, e créditos a terceiros envolvidos direta ou indiretamente.
