using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;

using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;

public class Fractal : MonoBehaviour
{
    [SerializeField, Range(1, 8)] int depth = 4;

    [SerializeField] Mesh mesh;
    [SerializeField] Material material;

    private static float3[] directions = {
        up(), right(), left(), forward(), back()
    };

    private static quaternion[] rotations = {
        quaternion.identity,
		quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
		quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
    };

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    private struct UpdateFractalLevelJob : IJobFor {
        public float spinAngleDelta;
        public float scale;

        [ReadOnly]
        public NativeArray<FractalPart> parentParts;

        public NativeArray<FractalPart> levelParts;

        [WriteOnly]
        public NativeArray<float3x4> levelMatricies;

        public void Execute(int i) {
            FractalPart parent = parentParts[i / 5];
            FractalPart part = levelParts[i];

            part.spinAngle += spinAngleDelta;
            part.worldRotation = mul(parent.worldRotation, mul(part.rotation, quaternion.RotateY(part.spinAngle)));
            part.worldPosition = 
                parent.worldPosition + 
                mul(parent.worldRotation, (1.5f * scale * part.direction));

            levelParts[i] = part;

            float3x3 r = float3x3(part.worldRotation) * scale;
            levelMatricies[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        } 
    } 

    private struct FractalPart {
        public float3 direction, worldPosition;
        public quaternion rotation, worldRotation;
        public float spinAngle;
    };

    ComputeBuffer[] matriciesBuffers;

    static readonly int matriciesId = Shader.PropertyToID("_Matricies");
    static MaterialPropertyBlock propertyBlock;

    NativeArray<FractalPart>[] parts;
    NativeArray<float3x4>[] matricies;

    private void OnEnable() {
        parts = new NativeArray<FractalPart>[depth];
        matricies = new NativeArray<float3x4>[depth];

        matriciesBuffers = new ComputeBuffer[depth];
        // The bottom row of the transformation matrix will allways be (0, 0, 0, 1), no need to send it over to the GPU
        int stride = 12 * 4;
        for (int i = 0, length = 1; i < depth; ++i, length *= 5) {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matricies[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matriciesBuffers[i] = new ComputeBuffer(length, stride);
        }

        parts[0][0] = CreatePart(0); 
        for(int lvl = 1; lvl < parts.Length; ++lvl) {
            for (int partIndex = 0; partIndex < parts[lvl].Length; partIndex += 5) {
                for (int childIndex = 0; childIndex < 5; ++childIndex) {
                    parts[lvl][partIndex + childIndex] = CreatePart(childIndex);
                }
            }
        }
        
        propertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable() {
        for (int i = 0; i < depth; ++i) {
            matriciesBuffers[i].Release();
            parts[i].Dispose();
			matricies[i].Dispose();
        }
        parts = null;
        matricies = null;
        matriciesBuffers = null;
    }

    private void OnValidate() {
        if (parts != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }

    private FractalPart CreatePart(int childIndex) => new FractalPart {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };

    private void Update() {
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;

        FractalPart root = parts[0][0];
        root.spinAngle += spinAngleDelta;
        root.worldRotation = mul(transform.rotation, mul(root.rotation, quaternion.RotateY(root.spinAngle)));
        root.worldPosition = transform.position;
        parts[0][0] = root;

        float3x3 r = float3x3(root.worldRotation) * transform.lossyScale.x;
        matricies[0][0] = float3x4(r.c0, r.c1, r.c2, root.worldPosition);
        float scale = 1f;

        JobHandle jobHandle = default;
        for (int lvl = 1; lvl < parts.Length; ++lvl) {
            scale *= 0.5f;

            jobHandle = new UpdateFractalLevelJob {
                spinAngleDelta = spinAngleDelta,
                scale = scale,
                parentParts = parts[lvl - 1],
                levelParts = parts[lvl],
                levelMatricies = matricies[lvl],
            }.ScheduleParallel(parts[lvl].Length, 5, jobHandle);
        }
        jobHandle.Complete();

        Bounds bounds = new Bounds(root.worldPosition, 3f * float3(transform.lossyScale.x));
        for (int i = 0; i < matriciesBuffers.Length; ++i) {
            ComputeBuffer buffer = matriciesBuffers[i];
            buffer.SetData(matricies[i]);
            propertyBlock.SetBuffer(matriciesId, buffer);
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, propertyBlock);
        }
    }
}
