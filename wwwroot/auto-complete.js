window.shouldOpenAutoCompleteDropdownUpward = (element) => {
  if (!element) return false;

  const rect = element.getBoundingClientRect();
  const viewportHeight = window.innerHeight;

  const spaceBelow = viewportHeight - rect.bottom;
  const spaceAbove = rect.top;

  // If there's not enough space below but there's more space above
  return spaceBelow < 300 && spaceAbove > spaceBelow;
};

window.isAutoCompleteOptionsListScrolledToBottom = function (element) {
  if (!element) return false;

  const scrollTop = element.scrollTop;
  const scrollHeight = element.scrollHeight;
  const clientHeight = element.clientHeight;

  // Check if scrolled to bottom (with 10px threshold)
  return scrollTop + clientHeight >= scrollHeight - 10;
};
