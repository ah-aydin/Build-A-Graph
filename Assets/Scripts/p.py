l = ['Wave',
'MultiWave',
'Ripple',
'Sphere',
'DonutKindOf',
'Onion',
'SpaceStation',
'TwistedOnion',
'Torus',
'TwistingTorus',]

for i in range(len(l)):
    for j in range(len(l)):
        if i == j: continue
        print(f'#pragma kernel {l[i]}To{l[j]}Kernel')