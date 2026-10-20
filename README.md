# PokeProof

PokeProof is a Windows WPF application for preparing video proof files. It can queue videos, copy and optionally rename them, add trade-history text, and compress them with FFmpeg using an AMD or NVIDIA encoder.

The queue displays each video's processing status and progress, as well as overall progress for the full batch.

The interface can be switched between German and English from the language selector in the window header.

## Video compression

AMD compression uses HEVC hardware encoding through AMF with QVBR quality level 51, the encoder's highest quality setting. This prioritizes image fidelity over file size while still using hardware compression. It preserves the input orientation, resolution, and frame timing instead of forcing every video to landscape Full HD. Compressed videos are muted; output size varies with the content. The encode remains lossy; truly lossless output would require a much larger file or copying the original without re-encoding.

## Build and run

Requirements:

- Windows and Visual Studio 2022 with the .NET desktop development workload
- The .NET Framework 4.8.1 Developer Pack
- NuGet CLI (`nuget.exe`) available on `PATH`

From the repository root, restore the legacy `packages.config` dependencies, build the solution, then run the executable:

```powershell
nuget restore .\PokeProof.sln
msbuild .\PokeProof.sln
.\bin\Debug\PokeProof.exe
```

Manifest signing is disabled for local builds because the signing certificate is not included in the repository. Configure your own certificate before publishing a signed ClickOnce deployment.
