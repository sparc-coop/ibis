export function MorphBlobs() {
    const morph1 = KUTE.fromTo('#blob1',
        { path: '#blob1' },
        { path: '#blob2' },
        { repeat: 100, duration: 3000, yoyo: true });
    const morph2 = KUTE.fromTo('#blob1',
        { path: '#blob1' },
        { path: '#blob3' },
        { repeat: 100, duration: 3000, yoyo: true });
    morph1.start();
    morph2.start();
}