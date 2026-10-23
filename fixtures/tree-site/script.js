document.addEventListener('DOMContentLoaded', () => {
    const leafCounter = document.getElementById('leaf-counter');
    let leafCount = 0;

    leafCounter.addEventListener('click', () => {
        leafCount++;
        leafCounter.textContent = `Count Leaves (${leafCount})`;
    });

    const treeCards = document.querySelectorAll('.tree-card');
    treeCards.forEach(card => {
        card.addEventListener('click', () => {
            alert(`You clicked on a ${card.getAttribute('data-type')} tree!`);
        });
    });
});