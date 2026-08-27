import { Canvas, useFrame } from '@react-three/fiber'
import { Float, MeshDistortMaterial, Sparkles } from '@react-three/drei'
import { useMemo, useRef } from 'react'
import type { Group, Mesh } from 'three'
import * as THREE from 'three'

function NeuralCore() {
  const group = useRef<Group>(null)
  const core = useRef<Mesh>(null)
  useFrame((state, delta) => {
    if (!group.current || !core.current) return
    group.current.rotation.y += delta * 0.09
    group.current.rotation.x = THREE.MathUtils.lerp(group.current.rotation.x, state.pointer.y * 0.12, 0.04)
    group.current.rotation.y = THREE.MathUtils.lerp(group.current.rotation.y, state.pointer.x * 0.18 + state.clock.elapsedTime * 0.09, 0.04)
    const pulse = 1 + Math.sin(state.clock.elapsedTime * 2.2) * 0.035
    core.current.scale.setScalar(pulse)
  })
  const nodes = useMemo(() => Array.from({ length: 28 }, (_, i) => {
    const phi = Math.acos(-1 + (2 * i) / 28)
    const theta = Math.sqrt(28 * Math.PI) * phi
    return [2.05 * Math.cos(theta) * Math.sin(phi), 2.05 * Math.sin(theta) * Math.sin(phi), 2.05 * Math.cos(phi)] as [number, number, number]
  }), [])

  return (
    <group ref={group}>
      <Float speed={1.4} rotationIntensity={0.25} floatIntensity={0.35}>
        <mesh ref={core}>
          <icosahedronGeometry args={[1.15, 6]} />
          <MeshDistortMaterial color="#071426" emissive="#2de2ff" emissiveIntensity={0.45} roughness={0.15} metalness={0.72} distort={0.28} speed={2.1} wireframe />
        </mesh>
        <mesh rotation={[0.8, 0.1, 0.5]}>
          <torusGeometry args={[1.62, 0.012, 8, 180]} />
          <meshBasicMaterial color="#7cf7ff" transparent opacity={0.55} />
        </mesh>
        <mesh rotation={[1.25, 0.5, 0.1]}>
          <torusGeometry args={[1.82, 0.008, 8, 180]} />
          <meshBasicMaterial color="#a981ff" transparent opacity={0.42} />
        </mesh>
        {nodes.map((position, i) => (
          <mesh key={i} position={position}>
            <sphereGeometry args={[i % 5 === 0 ? 0.045 : 0.022, 10, 10]} />
            <meshBasicMaterial color={i % 3 === 0 ? '#a981ff' : '#6cf0ff'} />
          </mesh>
        ))}
      </Float>
      <Sparkles count={110} scale={7} size={1.6} speed={0.35} color="#8ef7ff" opacity={0.65} />
    </group>
  )
}

export function NeuralScene() {
  return (
    <div className="neural-scene" aria-hidden="true">
      <Canvas dpr={[1, 1.5]} camera={{ position: [0, 0, 5.6], fov: 48 }} gl={{ antialias: true, alpha: true, powerPreference: 'high-performance' }}>
        <ambientLight intensity={0.25} />
        <pointLight color="#6cf0ff" intensity={18} position={[2, 2, 3]} />
        <pointLight color="#9b6dff" intensity={14} position={[-3, -2, 2]} />
        <NeuralCore />
      </Canvas>
      <div className="core-vignette" />
      <div className="core-scanline" />
    </div>
  )
}

