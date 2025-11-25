# Snapper

Ferramenta simples de **edição** para alinhar automaticamente (snap) todos os filhos de um GameObject a um grid de 1 unidade.

Ideal para organizar rapidamente níveis, tabuleiros e elementos posicionados manualmente.

---

## 📌 O que este script faz

- Percorre **todos os filhos** do objeto onde está anexado.
- Para cada filho:
  - Arredonda `x` e `y` para o inteiro mais próximo.
  - Mantém o valor original de `z`.
- Ignora o objeto raiz (onde o script está).
- Exibe um log no Console após executar.

---

## 🧭 Como usar

1. Selecione o GameObject que contém o script `Snapper`.
2. No Inspector, abra o menu de contexto do componente (três pontinhos ou clique direito no título).
3. Clique em **"Snap children to grid (1 unit)"**.
4. Todos os filhos serão automaticamente alinhados ao grid.

---

## 🧩 Limitações

- Sempre usa grid de **1 unidade** (não configurável neste script).
- Atua apenas em **modo Editor** (via ContextMenu).
- Não verifica colisões nem mantém offsets personalizados.

---

## 🛠 Possíveis melhorias futuras

- Grid configurável (ex.: 0.5, 1, 2 unidades).
- Snapping opcional apenas no eixo X ou Y.
- Função para snap por layer específica.
- Undo automático através de API do Editor.

---