using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Graphing;
using UnityEditor.Graphs;
using UnityEngine;              // Vector3,4
using UnityEditor.ShaderGraph;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

namespace UnityEditor.Experimental.Rendering.HDPipeline
{
    static class HDRPShaderStructs
    {
        internal struct AttributesMesh
        {
            [Semantic("POSITION")]                  Vector3 positionOS;
            [Semantic("NORMAL")][Optional]          Vector3 normalOS;
            [Semantic("TANGENT")][Optional]         Vector4 tangentOS;       // Stores bi-tangent sign in w
            [Semantic("TEXCOORD0")][Optional]       Vector4 uv0;
            [Semantic("TEXCOORD1")][Optional]       Vector4 uv1;
            [Semantic("TEXCOORD2")][Optional]       Vector4 uv2;
            [Semantic("TEXCOORD3")][Optional]       Vector4 uv3;
            [Semantic("COLOR")][Optional]           Vector4 color;
            [Semantic("INSTANCEID_SEMANTIC")] [PreprocessorIf("UNITY_ANY_INSTANCING_ENABLED")] uint instanceID;
        };

        [InterpolatorPack]
        internal struct VaryingsMeshToPS
        {
            [Semantic("SV_Position")]                                               Vector4 positionCS;
            [Optional]                                                              Vector3 positionRWS;
            [Optional]                                                              Vector3 normalWS;
            [Optional]                                                              Vector4 tangentWS;      // w contain mirror sign
            [Optional]                                                              Vector4 texCoord0;
            [Optional]                                                              Vector4 texCoord1;
            [Optional]                                                              Vector4 texCoord2;
            [Optional]                                                              Vector4 texCoord3;
            [Optional]                                                              Vector4 color;
            [Semantic("CUSTOM_INSTANCE_ID")] [PreprocessorIf("UNITY_ANY_INSTANCING_ENABLED")] uint instanceID;
            [Optional][Semantic("FRONT_FACE_SEMANTIC")][OverrideType("FRONT_FACE_TYPE")][PreprocessorIf("SHADER_STAGE_FRAGMENT")] bool cullFace;

            public static Dependency[] tessellationDependencies = new Dependency[]
            {
                new Dependency("VaryingsMeshToPS.positionRWS",       "VaryingsMeshToDS.positionRWS"),
                new Dependency("VaryingsMeshToPS.normalWS",         "VaryingsMeshToDS.normalWS"),
                new Dependency("VaryingsMeshToPS.tangentWS",        "VaryingsMeshToDS.tangentWS"),
                new Dependency("VaryingsMeshToPS.texCoord0",        "VaryingsMeshToDS.texCoord0"),
                new Dependency("VaryingsMeshToPS.texCoord1",        "VaryingsMeshToDS.texCoord1"),
                new Dependency("VaryingsMeshToPS.texCoord2",        "VaryingsMeshToDS.texCoord2"),
                new Dependency("VaryingsMeshToPS.texCoord3",        "VaryingsMeshToDS.texCoord3"),
                new Dependency("VaryingsMeshToPS.color",            "VaryingsMeshToDS.color"),
                new Dependency("VaryingsMeshToPS.instanceID",       "VaryingsMeshToDS.instanceID"),
            };

            public static Dependency[] standardDependencies = new Dependency[]
            {
                new Dependency("VaryingsMeshToPS.positionRWS",       "AttributesMesh.positionOS"),
                new Dependency("VaryingsMeshToPS.normalWS",         "AttributesMesh.normalOS"),
                new Dependency("VaryingsMeshToPS.tangentWS",        "AttributesMesh.tangentOS"),
                new Dependency("VaryingsMeshToPS.texCoord0",        "AttributesMesh.uv0"),
                new Dependency("VaryingsMeshToPS.texCoord1",        "AttributesMesh.uv1"),
                new Dependency("VaryingsMeshToPS.texCoord2",        "AttributesMesh.uv2"),
                new Dependency("VaryingsMeshToPS.texCoord3",        "AttributesMesh.uv3"),
                new Dependency("VaryingsMeshToPS.color",            "AttributesMesh.color"),
                new Dependency("VaryingsMeshToPS.instanceID",       "AttributesMesh.instanceID"),
            };
        };

        [InterpolatorPack]
        internal struct VaryingsMeshToDS
        {
            Vector3 positionRWS;
            Vector3 normalWS;
            [Optional]      Vector4 tangentWS;
            [Optional]      Vector4 texCoord0;
            [Optional]      Vector4 texCoord1;
            [Optional]      Vector4 texCoord2;
            [Optional]      Vector4 texCoord3;
            [Optional]      Vector4 color;
            [Semantic("CUSTOM_INSTANCE_ID")] [PreprocessorIf("UNITY_ANY_INSTANCING_ENABLED")] uint instanceID;

            public static Dependency[] tessellationDependencies = new Dependency[]
            {
                new Dependency("VaryingsMeshToDS.tangentWS",     "VaryingsMeshToPS.tangentWS"),
                new Dependency("VaryingsMeshToDS.texCoord0",     "VaryingsMeshToPS.texCoord0"),
                new Dependency("VaryingsMeshToDS.texCoord1",     "VaryingsMeshToPS.texCoord1"),
                new Dependency("VaryingsMeshToDS.texCoord2",     "VaryingsMeshToPS.texCoord2"),
                new Dependency("VaryingsMeshToDS.texCoord3",     "VaryingsMeshToPS.texCoord3"),
                new Dependency("VaryingsMeshToDS.color",         "VaryingsMeshToPS.color"),
                new Dependency("VaryingsMeshToDS.instanceID",    "VaryingsMeshToPS.instanceID"),
            };
        };

        internal struct FragInputs
        {
            public static Dependency[] dependencies = new Dependency[]
            {
                new Dependency("FragInputs.positionRWS",        "VaryingsMeshToPS.positionRWS"),
                new Dependency("FragInputs.worldToTangent",     "VaryingsMeshToPS.tangentWS"),
                new Dependency("FragInputs.worldToTangent",     "VaryingsMeshToPS.normalWS"),
                new Dependency("FragInputs.texCoord0",          "VaryingsMeshToPS.texCoord0"),
                new Dependency("FragInputs.texCoord1",          "VaryingsMeshToPS.texCoord1"),
                new Dependency("FragInputs.texCoord2",          "VaryingsMeshToPS.texCoord2"),
                new Dependency("FragInputs.texCoord3",          "VaryingsMeshToPS.texCoord3"),
                new Dependency("FragInputs.color",              "VaryingsMeshToPS.color"),
                new Dependency("FragInputs.isFrontFace",        "VaryingsMeshToPS.cullFace"),
            };
        };

        // this describes the input to the pixel shader graph eval
        internal struct SurfaceDescriptionInputs
        {
            [Optional] Vector3 ObjectSpaceNormal;
            [Optional] Vector3 ViewSpaceNormal;
            [Optional] Vector3 WorldSpaceNormal;
            [Optional] Vector3 TangentSpaceNormal;

            [Optional] Vector3 ObjectSpaceTangent;
            [Optional] Vector3 ViewSpaceTangent;
            [Optional] Vector3 WorldSpaceTangent;
            [Optional] Vector3 TangentSpaceTangent;

            [Optional] Vector3 ObjectSpaceBiTangent;
            [Optional] Vector3 ViewSpaceBiTangent;
            [Optional] Vector3 WorldSpaceBiTangent;
            [Optional] Vector3 TangentSpaceBiTangent;

            [Optional] Vector3 ObjectSpaceViewDirection;
            [Optional] Vector3 ViewSpaceViewDirection;
            [Optional] Vector3 WorldSpaceViewDirection;
            [Optional] Vector3 TangentSpaceViewDirection;

            [Optional] Vector3 ObjectSpacePosition;
            [Optional] Vector3 ViewSpacePosition;
            [Optional] Vector3 WorldSpacePosition;
            [Optional] Vector3 TangentSpacePosition;

            [Optional] Vector4 ScreenPosition;
            [Optional] Vector4 uv0;
            [Optional] Vector4 uv1;
            [Optional] Vector4 uv2;
            [Optional] Vector4 uv3;
            [Optional] Vector4 VertexColor;
            [Optional] float FaceSign;

            public static Dependency[] dependencies = new Dependency[]
            {
                new Dependency("SurfaceDescriptionInputs.WorldSpaceNormal",          "FragInputs.worldToTangent"),
                new Dependency("SurfaceDescriptionInputs.ObjectSpaceNormal",         "SurfaceDescriptionInputs.WorldSpaceNormal"),
                new Dependency("SurfaceDescriptionInputs.ViewSpaceNormal",           "SurfaceDescriptionInputs.WorldSpaceNormal"),

                new Dependency("SurfaceDescriptionInputs.WorldSpaceTangent",         "FragInputs.worldToTangent"),
                new Dependency("SurfaceDescriptionInputs.ObjectSpaceTangent",        "SurfaceDescriptionInputs.WorldSpaceTangent"),
                new Dependency("SurfaceDescriptionInputs.ViewSpaceTangent",          "SurfaceDescriptionInputs.WorldSpaceTangent"),

                new Dependency("SurfaceDescriptionInputs.WorldSpaceBiTangent",       "FragInputs.worldToTangent"),
                new Dependency("SurfaceDescriptionInputs.ObjectSpaceBiTangent",      "SurfaceDescriptionInputs.WorldSpaceBiTangent"),
                new Dependency("SurfaceDescriptionInputs.ViewSpaceBiTangent",        "SurfaceDescriptionInputs.WorldSpaceBiTangent"),

                new Dependency("SurfaceDescriptionInputs.WorldSpacePosition",        "FragInputs.positionRWS"),
                new Dependency("SurfaceDescriptionInputs.ObjectSpacePosition",       "FragInputs.positionRWS"),
                new Dependency("SurfaceDescriptionInputs.ViewSpacePosition",         "FragInputs.positionRWS"),

                new Dependency("SurfaceDescriptionInputs.WorldSpaceViewDirection",   "FragInputs.positionRWS"),                   // we build WorldSpaceViewDirection using FragInputs.positionRWS in GetWorldSpaceNormalizeViewDir()
                new Dependency("SurfaceDescriptionInputs.ObjectSpaceViewDirection",  "SurfaceDescriptionInputs.WorldSpaceViewDirection"),
                new Dependency("SurfaceDescriptionInputs.ViewSpaceViewDirection",    "SurfaceDescriptionInputs.WorldSpaceViewDirection"),
                new Dependency("SurfaceDescriptionInputs.TangentSpaceViewDirection", "SurfaceDescriptionInputs.WorldSpaceViewDirection"),
                new Dependency("SurfaceDescriptionInputs.TangentSpaceViewDirection", "SurfaceDescriptionInputs.WorldSpaceTangent"),
                new Dependency("SurfaceDescriptionInputs.TangentSpaceViewDirection", "SurfaceDescriptionInputs.WorldSpaceBiTangent"),
                new Dependency("SurfaceDescriptionInputs.TangentSpaceViewDirection", "SurfaceDescriptionInputs.WorldSpaceNormal"),

                new Dependency("SurfaceDescriptionInputs.ScreenPosition",            "SurfaceDescriptionInputs.WorldSpacePosition"),
                new Dependency("SurfaceDescriptionInputs.uv0",                       "FragInputs.texCoord0"),
                new Dependency("SurfaceDescriptionInputs.uv1",                       "FragInputs.texCoord1"),
                new Dependency("SurfaceDescriptionInputs.uv2",                       "FragInputs.texCoord2"),
                new Dependency("SurfaceDescriptionInputs.uv3",                       "FragInputs.texCoord3"),
                new Dependency("SurfaceDescriptionInputs.VertexColor",               "FragInputs.color"),
                new Dependency("SurfaceDescriptionInputs.FaceSign",                  "FragInputs.isFrontFace"),

                new Dependency("DepthOffset", "FragInputs.positionRWS"),
            };
        };

        // this describes the input to the pixel shader graph eval
        internal struct VertexDescriptionInputs
        {
            [Optional] Vector3 ObjectSpaceNormal;
            [Optional] Vector3 ViewSpaceNormal;
            [Optional] Vector3 WorldSpaceNormal;
            [Optional] Vector3 TangentSpaceNormal;

            [Optional] Vector3 ObjectSpaceTangent;
            [Optional] Vector3 ViewSpaceTangent;
            [Optional] Vector3 WorldSpaceTangent;
            [Optional] Vector3 TangentSpaceTangent;

            [Optional] Vector3 ObjectSpaceBiTangent;
            [Optional] Vector3 ViewSpaceBiTangent;
            [Optional] Vector3 WorldSpaceBiTangent;
            [Optional] Vector3 TangentSpaceBiTangent;

            [Optional] Vector3 ObjectSpaceViewDirection;
            [Optional] Vector3 ViewSpaceViewDirection;
            [Optional] Vector3 WorldSpaceViewDirection;
            [Optional] Vector3 TangentSpaceViewDirection;

            [Optional] Vector3 ObjectSpacePosition;
            [Optional] Vector3 ViewSpacePosition;
            [Optional] Vector3 WorldSpacePosition;
            [Optional] Vector3 TangentSpacePosition;

            [Optional] Vector4 ScreenPosition;
            [Optional] Vector4 uv0;
            [Optional] Vector4 uv1;
            [Optional] Vector4 uv2;
            [Optional] Vector4 uv3;
            [Optional] Vector4 VertexColor;

            public static Dependency[] dependencies = new Dependency[]
            {                                                                       // TODO: NOCHECKIN: these dependencies are not correct for vertex pass
                new Dependency("VertexDescriptionInputs.ObjectSpaceNormal",         "AttributesMesh.normalOS"),
                new Dependency("VertexDescriptionInputs.WorldSpaceNormal",          "AttributesMesh.normalOS"),
                new Dependency("VertexDescriptionInputs.ViewSpaceNormal",           "VertexDescriptionInputs.WorldSpaceNormal"),

                new Dependency("VertexDescriptionInputs.ObjectSpaceTangent",        "AttributesMesh.tangentOS"),
                new Dependency("VertexDescriptionInputs.WorldSpaceTangent",         "AttributesMesh.tangentOS"),
                new Dependency("VertexDescriptionInputs.ViewSpaceTangent",          "VertexDescriptionInputs.WorldSpaceTangent"),

                new Dependency("VertexDescriptionInputs.ObjectSpaceBiTangent",      "AttributesMesh.normalOS"),
                new Dependency("VertexDescriptionInputs.ObjectSpaceBiTangent",      "AttributesMesh.tangentOS"),
                new Dependency("VertexDescriptionInputs.WorldSpaceBiTangent",       "VertexDescriptionInputs.ObjectSpaceBiTangent"),
                new Dependency("VertexDescriptionInputs.ViewSpaceBiTangent",        "VertexDescriptionInputs.WorldSpaceBiTangent"),

                new Dependency("VertexDescriptionInputs.ObjectSpacePosition",       "AttributesMesh.positionOS"),
                new Dependency("VertexDescriptionInputs.WorldSpacePosition",        "AttributesMesh.positionOS"),
                new Dependency("VertexDescriptionInputs.ViewSpacePosition",         "VertexDescriptionInputs.WorldSpacePosition"),

                new Dependency("VertexDescriptionInputs.WorldSpaceViewDirection",   "VertexDescriptionInputs.WorldSpacePosition"),
                new Dependency("VertexDescriptionInputs.ObjectSpaceViewDirection",  "VertexDescriptionInputs.WorldSpaceViewDirection"),
                new Dependency("VertexDescriptionInputs.ViewSpaceViewDirection",    "VertexDescriptionInputs.WorldSpaceViewDirection"),
                new Dependency("VertexDescriptionInputs.TangentSpaceViewDirection", "VertexDescriptionInputs.WorldSpaceViewDirection"),
                new Dependency("VertexDescriptionInputs.TangentSpaceViewDirection", "VertexDescriptionInputs.WorldSpaceTangent"),
                new Dependency("VertexDescriptionInputs.TangentSpaceViewDirection", "VertexDescriptionInputs.WorldSpaceBiTangent"),
                new Dependency("VertexDescriptionInputs.TangentSpaceViewDirection", "VertexDescriptionInputs.WorldSpaceNormal"),

                new Dependency("VertexDescriptionInputs.ScreenPosition",            "VertexDescriptionInputs.WorldSpacePosition"),
                new Dependency("VertexDescriptionInputs.uv0",                       "AttributesMesh.uv0"),
                new Dependency("VertexDescriptionInputs.uv1",                       "AttributesMesh.uv1"),
                new Dependency("VertexDescriptionInputs.uv2",                       "AttributesMesh.uv2"),
                new Dependency("VertexDescriptionInputs.uv3",                       "AttributesMesh.uv3"),
                new Dependency("VertexDescriptionInputs.VertexColor",               "AttributesMesh.color"),
            };
        };

        // TODO: move this out of HDRPShaderStructs
        static public void AddActiveFieldsFromVertexGraphRequirements(HashSet<string> activeFields, ShaderGraphRequirements requirements)
        {
            if (requirements.requiresScreenPosition)
            {
                activeFields.Add("VertexDescriptionInputs.ScreenPosition");
            }

            if (requirements.requiresVertexColor)
            {
                activeFields.Add("VertexDescriptionInputs.VertexColor");
            }

            if (requirements.requiresNormal != 0)
            {
                if ((requirements.requiresNormal & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("VertexDescriptionInputs.ObjectSpaceNormal");

                if ((requirements.requiresNormal & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("VertexDescriptionInputs.ViewSpaceNormal");

                if ((requirements.requiresNormal & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("VertexDescriptionInputs.WorldSpaceNormal");

                if ((requirements.requiresNormal & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("VertexDescriptionInputs.TangentSpaceNormal");
            }

            if (requirements.requiresTangent != 0)
            {
                if ((requirements.requiresTangent & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("VertexDescriptionInputs.ObjectSpaceTangent");

                if ((requirements.requiresTangent & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("VertexDescriptionInputs.ViewSpaceTangent");

                if ((requirements.requiresTangent & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("VertexDescriptionInputs.WorldSpaceTangent");

                if ((requirements.requiresTangent & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("VertexDescriptionInputs.TangentSpaceTangent");
            }

            if (requirements.requiresBitangent != 0)
            {
                if ((requirements.requiresBitangent & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("VertexDescriptionInputs.ObjectSpaceBiTangent");

                if ((requirements.requiresBitangent & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("VertexDescriptionInputs.ViewSpaceBiTangent");

                if ((requirements.requiresBitangent & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("VertexDescriptionInputs.WorldSpaceBiTangent");

                if ((requirements.requiresBitangent & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("VertexDescriptionInputs.TangentSpaceBiTangent");
            }

            if (requirements.requiresViewDir != 0)
            {
                if ((requirements.requiresViewDir & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("VertexDescriptionInputs.ObjectSpaceViewDirection");

                if ((requirements.requiresViewDir & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("VertexDescriptionInputs.ViewSpaceViewDirection");

                if ((requirements.requiresViewDir & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("VertexDescriptionInputs.WorldSpaceViewDirection");

                if ((requirements.requiresViewDir & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("VertexDescriptionInputs.TangentSpaceViewDirection");
            }

            if (requirements.requiresPosition != 0)
            {
                if ((requirements.requiresPosition & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("VertexDescriptionInputs.ObjectSpacePosition");

                if ((requirements.requiresPosition & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("VertexDescriptionInputs.ViewSpacePosition");

                if ((requirements.requiresPosition & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("VertexDescriptionInputs.WorldSpacePosition");

                if ((requirements.requiresPosition & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("VertexDescriptionInputs.TangentSpacePosition");
            }

            foreach (var channel in requirements.requiresMeshUVs.Distinct())
            {
                activeFields.Add("VertexDescriptionInputs." + channel.GetUVName());
            }
        }

        // TODO: move this out of HDRPShaderStructs
        static public void AddActiveFieldsFromPixelGraphRequirements(HashSet<string> activeFields, ShaderGraphRequirements requirements)
        {
            if (requirements.requiresScreenPosition)
            {
                activeFields.Add("SurfaceDescriptionInputs.ScreenPosition");
            }

            if (requirements.requiresVertexColor)
            {
                activeFields.Add("SurfaceDescriptionInputs.VertexColor");
            }

            if (requirements.requiresFaceSign)
            {
                activeFields.Add("SurfaceDescriptionInputs.FaceSign");
            }

            if (requirements.requiresNormal != 0)
            {
                if ((requirements.requiresNormal & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ObjectSpaceNormal");

                if ((requirements.requiresNormal & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ViewSpaceNormal");

                if ((requirements.requiresNormal & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.WorldSpaceNormal");

                if ((requirements.requiresNormal & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.TangentSpaceNormal");
            }

            if (requirements.requiresTangent != 0)
            {
                if ((requirements.requiresTangent & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ObjectSpaceTangent");

                if ((requirements.requiresTangent & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ViewSpaceTangent");

                if ((requirements.requiresTangent & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.WorldSpaceTangent");

                if ((requirements.requiresTangent & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.TangentSpaceTangent");
            }

            if (requirements.requiresBitangent != 0)
            {
                if ((requirements.requiresBitangent & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ObjectSpaceBiTangent");

                if ((requirements.requiresBitangent & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ViewSpaceBiTangent");

                if ((requirements.requiresBitangent & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.WorldSpaceBiTangent");

                if ((requirements.requiresBitangent & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.TangentSpaceBiTangent");
            }

            if (requirements.requiresViewDir != 0)
            {
                if ((requirements.requiresViewDir & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ObjectSpaceViewDirection");

                if ((requirements.requiresViewDir & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ViewSpaceViewDirection");

                if ((requirements.requiresViewDir & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.WorldSpaceViewDirection");

                if ((requirements.requiresViewDir & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.TangentSpaceViewDirection");
            }

            if (requirements.requiresPosition != 0)
            {
                if ((requirements.requiresPosition & NeededCoordinateSpace.Object) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ObjectSpacePosition");

                if ((requirements.requiresPosition & NeededCoordinateSpace.View) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.ViewSpacePosition");

                if ((requirements.requiresPosition & NeededCoordinateSpace.World) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.WorldSpacePosition");

                if ((requirements.requiresPosition & NeededCoordinateSpace.Tangent) > 0)
                    activeFields.Add("SurfaceDescriptionInputs.TangentSpacePosition");
            }

            foreach (var channel in requirements.requiresMeshUVs.Distinct())
            {
                activeFields.Add("SurfaceDescriptionInputs." + channel.GetUVName());
            }
        }

        public static void AddRequiredFields(
            List<string> passRequiredFields,            // fields the pass requires
            HashSet<string> activeFields)
        {
            if (passRequiredFields != null)
            {
                foreach (var requiredField in passRequiredFields)
                {
                    activeFields.Add(requiredField);
                }
            }
        }
    };

    delegate void OnGeneratePassDelegate(IMasterNode masterNode, ref Pass pass);
    struct Pass
    {
        public string Name;
        public string LightMode;
        public string ShaderPassName;
        public List<string> Includes;
        public string TemplateName;
        public string MaterialName;
        public List<string> ExtraInstancingOptions;
        public List<string> ExtraDefines;
        public List<int> VertexShaderSlots;         // These control what slots are used by the pass vertex shader
        public List<int> PixelShaderSlots;          // These control what slots are used by the pass pixel shader
        public string CullOverride;
        public string BlendOverride;
        public string BlendOpOverride;
        public string ZTestOverride;
        public string ZWriteOverride;
        public string ColorMaskOverride;
        public List<string> StencilOverride;
        public List<string> RequiredFields;         // feeds into the dependency analysis
        public bool UseInPreview;

        // All these lists could probably be hashed to aid lookups.
        public bool VertexShaderUsesSlot(int slotId)
        {
            return VertexShaderSlots.Contains(slotId);
        }
        public bool PixelShaderUsesSlot(int slotId)
        {
            return PixelShaderSlots.Contains(slotId);
        }
        public void OnGeneratePass(IMasterNode masterNode)
        {
            if (OnGeneratePassImpl != null)
            {
                OnGeneratePassImpl(masterNode, ref this);
            }
        }
        public OnGeneratePassDelegate OnGeneratePassImpl;
    }

    static class HDSubShaderUtilities
    {

        public static bool GenerateShaderPass(AbstractMaterialNode masterNode, Pass pass, GenerationMode mode, SurfaceMaterialOptions materialOptions, HashSet<string> activeFields, ShaderGenerator result, List<string> sourceAssetDependencyPaths, bool vertexActive)
        {
            string templatePath = Path.Combine(HDUtils.GetHDRenderPipelinePath(), "Editor/Material");
            string templateLocation = Path.Combine(Path.Combine(Path.Combine(templatePath, pass.MaterialName), "ShaderGraph"), pass.TemplateName);
            if (!File.Exists(templateLocation))
            {
                // TODO: produce error here
                Debug.LogError("Template not found: " + templateLocation);
                return false;
            }

            bool debugOutput = false;

            //
            // Code generation that is not variant dependent
            //

            HDRPShaderStructs.AddRequiredFields(pass.RequiredFields, activeFields);

            var blendCode = new ShaderStringBuilder();
            var cullCode = new ShaderStringBuilder();
            var zTestCode = new ShaderStringBuilder();
            var zWriteCode = new ShaderStringBuilder();
            var zClipCode = new ShaderStringBuilder();
            var stencilCode = new ShaderStringBuilder();
            var colorMaskCode = new ShaderStringBuilder();
            HDSubShaderUtilities.BuildRenderStatesFromPassAndMaterialOptions(pass, materialOptions, blendCode, cullCode, zTestCode, zWriteCode, zClipCode, stencilCode, colorMaskCode);

            ShaderGenerator instancingOptions = new ShaderGenerator();
            {
                instancingOptions.AddShaderChunk("#pragma multi_compile_instancing", true);
                if (pass.ExtraInstancingOptions != null)
                {
                    foreach (var instancingOption in pass.ExtraInstancingOptions)
                        instancingOptions.AddShaderChunk(instancingOption);
                }
            }

            var shaderPassIncludes = new ShaderGenerator();
            if (pass.Includes != null)
            {
                foreach (var include in pass.Includes)
                    shaderPassIncludes.AddShaderChunk(include);
            }

            // Function Registry tracks functions to remove duplicates, it wraps a string builder that stores the combined function string
            // Will contains the function library for all variants.
            ShaderStringBuilder graphNodeFunctions = new ShaderStringBuilder();
            graphNodeFunctions.IncreaseIndent();
            var functionRegistry = new FunctionRegistry(graphNodeFunctions);

            // Build the list of active slots based on what the pass requires
            var pixelSlots = HDSubShaderUtilities.FindMaterialSlotsOnNode(pass.PixelShaderSlots, masterNode);
            var vertexSlots = HDSubShaderUtilities.FindMaterialSlotsOnNode(pass.VertexShaderSlots, masterNode);

            //
            // Code generation that is variant dependent
            //

            // Define shader string builder that are variant dependent
            string pixelGraphInputStructName = "SurfaceDescriptionInputs";
            string pixelGraphOutputStructName = "SurfaceDescription";
            string pixelGraphEvalFunctionName = "SurfaceDescriptionFunction";
            ShaderStringBuilder pixelGraphEvalFunction = new ShaderStringBuilder();
            ShaderStringBuilder pixelGraphOutputs = new ShaderStringBuilder();

            string vertexGraphInputStructName = "VertexDescriptionInputs";
            string vertexGraphOutputStructName = "VertexDescription";
            string vertexGraphEvalFunctionName = "VertexDescriptionFunction";
            ShaderStringBuilder vertexGraphEvalFunction = new ShaderStringBuilder();
            ShaderStringBuilder vertexGraphOutputs = new ShaderStringBuilder();

            ShaderGenerator pixelGraphInputs = new ShaderGenerator();
            ShaderGenerator vertexGraphInputs = new ShaderGenerator();
            ShaderGenerator defines = new ShaderGenerator();
            ShaderStringBuilder sharedPropertiesDeclarations = new ShaderStringBuilder();

            var interpolatorDefines = new ShaderGenerator();

            // grab all of the active nodes (for pixel and vertex graphs) per variant
            using (UnityEngine.Experimental.Rendering.ListPool<ShaderStringBuilder>.Get(out var stringBuilders))
            using (UnityEngine.Experimental.Rendering.ListPool<ShaderGenerator>.Get(out var generators))
            using (var vertexNodesPerVariant = KeywordSupport.NodesPerVariant.New())
            using (var pixelNodesPerVariant = KeywordSupport.NodesPerVariant.New())
            {
                stringBuilders.Add(pixelGraphEvalFunction);
                stringBuilders.Add(pixelGraphOutputs);
                stringBuilders.Add(vertexGraphEvalFunction);
                stringBuilders.Add(vertexGraphOutputs);
                stringBuilders.Add(sharedPropertiesDeclarations);
                generators.Add(pixelGraphInputs);
                generators.Add(vertexGraphInputs);
                generators.Add(defines);

                using (HashSetPool<KeywordSupport.KeywordSet>.Get(out var variants))
                {
                    KeywordSupport.CollectVariantsFor(variants, masterNode, pass.VertexShaderSlots);
                    KeywordSupport.CollectVariantsFor(variants, masterNode, pass.PixelShaderSlots);

                    KeywordSupport.CollectNodesPerVariantsFor(vertexNodesPerVariant, variants, masterNode, pass.VertexShaderSlots);
                    KeywordSupport.CollectNodesPerVariantsFor(pixelNodesPerVariant, variants, masterNode, pass.PixelShaderSlots);

                    // release KeywordSets in variants
                    foreach (var variant in variants)
                        variant.Dispose();
                    variants.Clear();
                }

                // graph requirements describe what the graph itself requires
                using (var pixelRequirementsPerVariant =
                    new KeywordSupport.RequirementsPerVariant(pixelNodesPerVariant, masterNode, ShaderStageCapability.Fragment,
                        false)) // TODO: is ShaderStageCapability.Fragment correct?
                using (var vertexRequirementsPerVariant =
                    new KeywordSupport.RequirementsPerVariant(vertexNodesPerVariant, masterNode, ShaderStageCapability.Vertex, false))
                using (UnityEngine.Experimental.Rendering.ListPool<KeywordSupport.KeywordSet>.Get(out var variants))
                {
                    var graphRequirementsPerVariant = pixelRequirementsPerVariant.Union(vertexRequirementsPerVariant);

                    // Get a list of variants and set the default (empty) variant as the last one
                    // We need this because for shader, this is the last one (#else)
                    variants.AddRange(graphRequirementsPerVariant.keywordSets);
                    var emptyVariantIndex = variants.FindIndex(v => v.keywords.Count == 0);
                    if (emptyVariantIndex >= 0)
                    {
                        var emptyVariant = variants[emptyVariantIndex];
                        variants.RemoveAt(emptyVariantIndex);
                        variants.Add(emptyVariant);
                    }

                    var isFirstVariant = true;

                    foreach (var variant in variants)
                    {
                        var pixelRequirements = pixelRequirementsPerVariant[variant];
                        var vertexRequirements = vertexRequirementsPerVariant[variant];
                        var pixelNodes = pixelNodesPerVariant[variant];
                        var vertexNodes = vertexNodesPerVariant[variant];
                        var graphRequirements = graphRequirementsPerVariant[variant];

                        var condition = string.Empty;
                        if (isFirstVariant)
                            condition = $"#if {string.Join(" ", variant.keywords)}";
                        else if (variant.keywords.Count == 0)
                            condition = "#else";
                        else
                            condition = $"#elif {string.Join(" ", variant.keywords)}";
                        isFirstVariant = false;

                        foreach (var stringBuilder in stringBuilders)
                            stringBuilder.AppendLine(condition);
                        foreach (var generator in generators)
                            generator.AddShaderChunk(condition);

                        // TODO: this can be a shared function for all HDRP master nodes -- From here through GraphUtil.GenerateSurfaceDescription(..)

                        // properties used by either pixel and vertex shader
                        PropertyCollector sharedProperties = new PropertyCollector();

                        // build the graph outputs structure to hold the results of each active slots (and fill out activeFields to indicate they are active)

                        // build initial requirements
                        HDRPShaderStructs.AddActiveFieldsFromPixelGraphRequirements(activeFields, pixelRequirements);

                        // build the graph outputs structure, and populate activeFields with the fields of that structure
                        GraphUtil.GenerateSurfaceDescriptionStruct(pixelGraphOutputs, pixelSlots, pixelGraphOutputStructName, activeFields);

                        // Build the graph evaluation code, to evaluate the specified slots
                        GraphUtil.GenerateSurfaceDescriptionFunction(
                            pixelNodes,
                            masterNode,
                            masterNode.owner as GraphData,
                            pixelGraphEvalFunction,
                            functionRegistry,
                            sharedProperties,
                            pixelRequirements,  // TODO : REMOVE UNUSED
                            mode,
                            pixelGraphEvalFunctionName,
                            pixelGraphOutputStructName,
                            null,
                            pixelSlots,
                            pixelGraphInputStructName);

                        // check for vertex animation -- enables HAVE_VERTEX_MODIFICATION
                        if (vertexActive)
                        {
                            vertexActive = true;
                            activeFields.Add("features.modifyMesh");
                            HDRPShaderStructs.AddActiveFieldsFromVertexGraphRequirements(activeFields, vertexRequirements);

                            // -------------------------------------
                            // Generate Output structure for Vertex Description function
                            GraphUtil.GenerateVertexDescriptionStruct(vertexGraphOutputs, vertexSlots, vertexGraphOutputStructName, activeFields);

                            // -------------------------------------
                            // Generate Vertex Description function
                            GraphUtil.GenerateVertexDescriptionFunction(
                                masterNode.owner as GraphData,
                                vertexGraphEvalFunction,
                                functionRegistry,
                                sharedProperties,
                                mode,
                                vertexNodes,
                                vertexSlots,
                                vertexGraphInputStructName,
                                vertexGraphEvalFunctionName,
                                vertexGraphOutputStructName);
                        }

                        sharedPropertiesDeclarations.AppendLines(sharedProperties.GetPropertiesDeclaration(1, mode));

                        // propagate active field requirements using dependencies
                        ShaderSpliceUtil.ApplyDependencies(
                            activeFields,
                            new List<Dependency[]>()
                            {
                                HDRPShaderStructs.FragInputs.dependencies,
                                HDRPShaderStructs.VaryingsMeshToPS.standardDependencies,
                                HDRPShaderStructs.SurfaceDescriptionInputs.dependencies,
                                HDRPShaderStructs.VertexDescriptionInputs.dependencies
                            });

                        // debug output all active fields
                        if (debugOutput)
                        {
                            interpolatorDefines.AddShaderChunk("// ACTIVE FIELDS:");
                            foreach (string f in activeFields)
                            {
                                interpolatorDefines.AddShaderChunk("//   " + f);
                            }
                        }

                        // build graph inputs structures
                        ShaderSpliceUtil.BuildType(typeof(HDRPShaderStructs.SurfaceDescriptionInputs), activeFields, pixelGraphInputs);
                        ShaderSpliceUtil.BuildType(typeof(HDRPShaderStructs.VertexDescriptionInputs), activeFields, vertexGraphInputs);

                        // Defines
                        {
                            defines.AddShaderChunk(string.Format("#define SHADERPASS {0}", pass.ShaderPassName), true);
                            if (pass.ExtraDefines != null)
                            {
                                foreach (var define in pass.ExtraDefines)
                                    defines.AddShaderChunk(define);
                            }
                            if (graphRequirements.requiresDepthTexture)
                                defines.AddShaderChunk("#define REQUIRE_DEPTH_TEXTURE");
                            if (graphRequirements.requiresCameraOpaqueTexture)
                                defines.AddShaderChunk("#define REQUIRE_OPAQUE_TEXTURE");
                            defines.AddGenerator(interpolatorDefines);
                        }
                    }

                    foreach (var stringBuilder in stringBuilders)
                        stringBuilder.AppendLine("#endif");
                    foreach (var generator in generators)
                        generator.AddShaderChunk("#endif");

                    graphRequirementsPerVariant.Dispose();
                }
            }


            // build graph code
            var graph = new ShaderGenerator();
            {
                graph.AddShaderChunk("// Shared Graph Properties (uniform inputs)");
                graph.AddShaderChunk(sharedPropertiesDeclarations.ToString());

                if (vertexActive)
                {
                    graph.AddShaderChunk("// Vertex Graph Inputs");
                    graph.Indent();
                    graph.AddGenerator(vertexGraphInputs);
                    graph.Deindent();
                    graph.AddShaderChunk("// Vertex Graph Outputs");
                    graph.Indent();
                    graph.AddShaderChunk(vertexGraphOutputs.ToString());
                    graph.Deindent();
                }

                graph.AddShaderChunk("// Pixel Graph Inputs");
                graph.Indent();
                graph.AddGenerator(pixelGraphInputs);
                graph.Deindent();
                graph.AddShaderChunk("// Pixel Graph Outputs");
                graph.Indent();
                graph.AddShaderChunk(pixelGraphOutputs.ToString());
                graph.Deindent();

                graph.AddShaderChunk("// Shared Graph Node Functions");
                graph.AddShaderChunk(graphNodeFunctions.ToString());

                if (vertexActive)
                {
                    graph.AddShaderChunk("// Vertex Graph Evaluation");
                    graph.Indent();
                    graph.AddShaderChunk(vertexGraphEvalFunction.ToString());
                    graph.Deindent();
                }

                graph.AddShaderChunk("// Pixel Graph Evaluation");
                graph.Indent();
                graph.AddShaderChunk(pixelGraphEvalFunction.ToString());
                graph.Deindent();
            }

            // build the hash table of all named fragments      TODO: could make this Dictionary<string, ShaderGenerator / string>  ?
            Dictionary<string, string> namedFragments = new Dictionary<string, string>();
            namedFragments.Add("InstancingOptions", instancingOptions.GetShaderString(0, false));
            namedFragments.Add("Defines", defines.GetShaderString(2, false));
            namedFragments.Add("Graph", graph.GetShaderString(2, false));
            namedFragments.Add("LightMode", pass.LightMode);
            namedFragments.Add("PassName", pass.Name);
            namedFragments.Add("Includes", shaderPassIncludes.GetShaderString(2, false));
            namedFragments.Add("Blending", blendCode.ToString());
            namedFragments.Add("Culling", cullCode.ToString());
            namedFragments.Add("ZTest", zTestCode.ToString());
            namedFragments.Add("ZWrite", zWriteCode.ToString());
            namedFragments.Add("ZClip", zClipCode.ToString());
            namedFragments.Add("Stencil", stencilCode.ToString());
            namedFragments.Add("ColorMask", colorMaskCode.ToString());
            namedFragments.Add("LOD", materialOptions.lod.ToString());

            // this is the format string for building the 'C# qualified assembly type names' for $buildType() commands
            string buildTypeAssemblyNameFormat = "UnityEditor.Experimental.Rendering.HDPipeline.HDRPShaderStructs+{0}, " + typeof(HDSubShaderUtilities).Assembly.FullName.ToString();

            string sharedTemplatePath = Path.Combine(Path.Combine(HDUtils.GetHDRenderPipelinePath(), "Editor"), "ShaderGraph");
            // process the template to generate the shader code for this pass
            ShaderSpliceUtil.TemplatePreprocessor templatePreprocessor =
                new ShaderSpliceUtil.TemplatePreprocessor(activeFields, namedFragments, debugOutput, sharedTemplatePath, sourceAssetDependencyPaths, buildTypeAssemblyNameFormat);

            templatePreprocessor.ProcessTemplateFile(templateLocation);

            result.AddShaderChunk(templatePreprocessor.GetShaderCode().ToString(), false);

            return true;
        }

        public static List<MaterialSlot> FindMaterialSlotsOnNode(IEnumerable<int> slots, AbstractMaterialNode node)
        {
            var activeSlots = new List<MaterialSlot>();
            if (slots != null)
            {
                foreach (var id in slots)
                {
                    MaterialSlot slot = node.FindSlot<MaterialSlot>(id);
                    if (slot != null)
                    {
                        activeSlots.Add(slot);
                    }
                }
            }
            return activeSlots;
        }

        public static void BuildRenderStatesFromPassAndMaterialOptions(
            Pass pass,
            SurfaceMaterialOptions materialOptions,
            ShaderStringBuilder blendCode,
            ShaderStringBuilder cullCode,
            ShaderStringBuilder zTestCode,
            ShaderStringBuilder zWriteCode,
            ShaderStringBuilder zClipCode,
            ShaderStringBuilder stencilCode,
            ShaderStringBuilder colorMaskCode)
        {
            if (pass.BlendOverride != null)
            {
                blendCode.AppendLine(pass.BlendOverride);
            }
            else
            {
                materialOptions.GetBlend(blendCode);
            }

            if (pass.BlendOpOverride != null)
            {
                blendCode.AppendLine(pass.BlendOpOverride);
            }

            if (pass.CullOverride != null)
            {
                cullCode.AppendLine(pass.CullOverride);
            }
            else
            {
                materialOptions.GetCull(cullCode);
            }

            if (pass.ZTestOverride != null)
            {
                zTestCode.AppendLine(pass.ZTestOverride);
            }
            else
            {
                materialOptions.GetDepthTest(zTestCode);
            }

            if (pass.ZWriteOverride != null)
            {
                zWriteCode.AppendLine(pass.ZWriteOverride);
            }
            else
            {
                materialOptions.GetDepthWrite(zWriteCode);
            }

            // No point in an override for this.
            materialOptions.GetDepthClip(zClipCode);

            if (pass.ColorMaskOverride != null)
            {
                colorMaskCode.AppendLine(pass.ColorMaskOverride);
            }
            else
            {
                // material option default is to not declare anything for color mask
            }

            if (pass.StencilOverride != null)
            {
                foreach (var str in pass.StencilOverride)
                {
                    stencilCode.AppendLine(str);
                }
            }
            else
            {
                stencilCode.AppendLine("// Default Stencil");
            }
        }

        public static HDMaterialTags BuildMaterialTags(HDRenderQueue.RenderQueueType renderQueueType,
                                                       int sortPriority,
                                                       bool alphaTest,
                                                       HDMaterialTags.RenderType renderType = HDMaterialTags.RenderType.HDLitShader)
        {
            return new HDMaterialTags
            {
                renderType = renderType,
                renderQueueIndex = HDRenderQueue.ChangeType(renderQueueType, sortPriority, alphaTest)
            };
        }

        public static HDMaterialTags BuildMaterialTags(SurfaceType surfaceType,
                                                       int sortPriority,
                                                       bool alphaTest,
                                                       HDMaterialTags.RenderType renderType = HDMaterialTags.RenderType.HDLitShader)
        {
            HDRenderQueue.RenderQueueType renderQueueType = HDRenderQueue.RenderQueueType.Opaque;

            if (surfaceType == SurfaceType.Transparent)
                renderQueueType = HDRenderQueue.RenderQueueType.Transparent;

            return BuildMaterialTags(renderQueueType, sortPriority, alphaTest, renderType);
        }

        public static SurfaceMaterialOptions BuildMaterialOptions(SurfaceType surfaceType,
                                                                  AlphaMode alphaMode,
                                                                  bool twoSided,
                                                                  bool refraction,
                                                                  bool offscreenTransparent)
        {
            SurfaceMaterialOptions materialOptions = new SurfaceMaterialOptions();
            if (surfaceType == SurfaceType.Opaque)
            {
                materialOptions.srcBlend = SurfaceMaterialOptions.BlendMode.One;
                materialOptions.dstBlend = SurfaceMaterialOptions.BlendMode.Zero;
                materialOptions.zTest = SurfaceMaterialOptions.ZTest.LEqual;
                materialOptions.zWrite = SurfaceMaterialOptions.ZWrite.On;
            }
            else
            {
                if (refraction)
                {
                    materialOptions.srcBlend = SurfaceMaterialOptions.BlendMode.One;
                    materialOptions.dstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                    materialOptions.alphaSrcBlend = SurfaceMaterialOptions.BlendMode.One;
                    materialOptions.alphaDstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                }
                else
                {
                    switch (alphaMode)
                    {
                        case AlphaMode.Alpha:
                            materialOptions.srcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.dstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                            materialOptions.alphaSrcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.alphaDstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                            break;
                        case AlphaMode.Additive:
                            materialOptions.srcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.dstBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.alphaSrcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.alphaDstBlend = SurfaceMaterialOptions.BlendMode.One;
                            break;
                        case AlphaMode.Premultiply:
                            materialOptions.srcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.dstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                            materialOptions.alphaSrcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.alphaDstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                            break;
                        // This isn't supported in HDRP.
                        case AlphaMode.Multiply:
                            materialOptions.srcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.dstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                            materialOptions.alphaSrcBlend = SurfaceMaterialOptions.BlendMode.One;
                            materialOptions.alphaDstBlend = SurfaceMaterialOptions.BlendMode.OneMinusSrcAlpha;
                            break;
                    }
                }

                if(offscreenTransparent)
                {
                    materialOptions.alphaSrcBlend = SurfaceMaterialOptions.BlendMode.Zero;
                }
                materialOptions.zTest = SurfaceMaterialOptions.ZTest.LEqual;
                materialOptions.zWrite = SurfaceMaterialOptions.ZWrite.Off;
            }

            materialOptions.cullMode = twoSided ? SurfaceMaterialOptions.CullMode.Off : SurfaceMaterialOptions.CullMode.Back;

            return materialOptions;
        }

        // Comment set of define for Forward Opaque pass in HDRP
        public static List<string> s_ExtraDefinesForwardOpaque = new List<string>()
        {
            "#pragma multi_compile _ DEBUG_DISPLAY",
            "#pragma multi_compile _ LIGHTMAP_ON",
            "#pragma multi_compile _ DIRLIGHTMAP_COMBINED",
            "#pragma multi_compile _ DYNAMICLIGHTMAP_ON",
            "#pragma multi_compile _ SHADOWS_SHADOWMASK",
            "#pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT",
            "#pragma multi_compile USE_FPTL_LIGHTLIST USE_CLUSTERED_LIGHTLIST",
            "#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH SHADOW_VERY_HIGH"
        };

        public static List<string> s_ExtraDefinesForwardTransparent = new List<string>()
        {
            "#pragma multi_compile _ DEBUG_DISPLAY",
            "#pragma multi_compile _ LIGHTMAP_ON",
            "#pragma multi_compile _ DIRLIGHTMAP_COMBINED",
            "#pragma multi_compile _ DYNAMICLIGHTMAP_ON",
            "#pragma multi_compile _ SHADOWS_SHADOWMASK",
            "#pragma multi_compile DECALS_OFF DECALS_3RT DECALS_4RT",
            "#define USE_CLUSTERED_LIGHTLIST",
            "#pragma multi_compile SHADOW_LOW SHADOW_MEDIUM SHADOW_HIGH SHADOW_VERY_HIGH"
        };

        public static List<string> s_ExtraDefinesForwardMaterialDepthOrMotion = new List<string>()
        {
            "#define WRITE_NORMAL_BUFFER",
            "#pragma multi_compile _ WRITE_MSAA_DEPTH"
        };

        public static List<string> s_ExtraDefinesDepthOrMotion = new List<string>()
        {
            "#pragma multi_compile _ WRITE_NORMAL_BUFFER",
            "#pragma multi_compile _ WRITE_MSAA_DEPTH"
        };

        public static string RenderQueueName(HDRenderQueue.RenderQueueType value)
        {
            switch (value)
            {
                case HDRenderQueue.RenderQueueType.Opaque:
                    return "Default";
                case HDRenderQueue.RenderQueueType.AfterPostProcessOpaque:
                    return "After Post-process";
                case HDRenderQueue.RenderQueueType.PreRefraction:
                    return "Before Refraction";
                case HDRenderQueue.RenderQueueType.Transparent:
                    return "Default";
                case HDRenderQueue.RenderQueueType.LowTransparent:
                    return "Low Resolution";
                case HDRenderQueue.RenderQueueType.AfterPostprocessTransparent:
                    return "After Post-process";

#if ENABLE_RAYTRACING
                case HDRenderQueue.RenderQueueType.RaytracingOpaque: return "Raytracing";
                case HDRenderQueue.RenderQueueType.RaytracingTransparent: return "Raytracing";
#endif
                default:
                    return "None";
            }
        }

        public static System.Collections.Generic.List<HDRenderQueue.RenderQueueType> GetRenderingPassList(bool opaque, bool needAfterPostProcess)
        {
            var result = new System.Collections.Generic.List<HDRenderQueue.RenderQueueType>();
            if (opaque)
            {
                result.Add(HDRenderQueue.RenderQueueType.Opaque);
                if (needAfterPostProcess)
                    result.Add(HDRenderQueue.RenderQueueType.AfterPostProcessOpaque);
#if ENABLE_RAYTRACING
                result.Add(HDRenderQueue.RenderQueueType.RaytracingOpaque);
#endif
            }
            else
            {
                result.Add(HDRenderQueue.RenderQueueType.PreRefraction);
                result.Add(HDRenderQueue.RenderQueueType.Transparent);
                result.Add(HDRenderQueue.RenderQueueType.LowTransparent);
                if (needAfterPostProcess)
                    result.Add(HDRenderQueue.RenderQueueType.AfterPostprocessTransparent);
#if ENABLE_RAYTRACING
                result.Add(HDRenderQueue.RenderQueueType.RaytracingTransparent);
#endif
            }

            return result;
        }

        public static void GetStencilStateForDepthOrMV(bool receiveDecals, bool receiveSSR, bool useObjectMotionVector, ref Pass pass)
        {
            int stencilWriteMask = (int)HDRenderPipeline.StencilBitMask.DecalsForwardOutputNormalBuffer;
            int stencilRef = receiveDecals ? (int)HDRenderPipeline.StencilBitMask.DecalsForwardOutputNormalBuffer : 0;

            stencilWriteMask |= (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR;
            stencilRef |= !receiveSSR ? (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR : 0;

            stencilWriteMask |= useObjectMotionVector ? (int)HDRenderPipeline.StencilBitMask.ObjectMotionVectors : 0;
            stencilRef |= useObjectMotionVector ? (int)HDRenderPipeline.StencilBitMask.ObjectMotionVectors : 0;

            if (stencilWriteMask != 0)
            {
                pass.StencilOverride = new List<string>()
                {
                    "// Stencil setup",
                    "Stencil",
                    "{",
                    string.Format("   WriteMask {0}", stencilWriteMask),
                    string.Format("   Ref  {0}", stencilRef),
                    "   Comp Always",
                    "   Pass Replace",
                    "}"
                };
            }
        }

        public static void GetStencilStateForForward(bool useSplitLighting, ref Pass pass)
        {
            pass.StencilOverride = new List<string>()
            {
                "// Stencil setup",
                "Stencil",
                "{",
                string.Format("   WriteMask {0}", (int) HDRenderPipeline.StencilBitMask.LightingMask),
                string.Format("   Ref  {0}", useSplitLighting ? (int)StencilLightingUsage.SplitLighting : (int)StencilLightingUsage.RegularLighting),
                "   Comp Always",
                "   Pass Replace",
                "}"
            };
        }

        public static void GetStencilStateForForwardUnlit(ref Pass pass)
        {
            pass.StencilOverride = new List<string>()
            {
                "// Stencil setup",
                "Stencil",
                "{",
                string.Format("   WriteMask {0}", (int) HDRenderPipeline.StencilBitMask.LightingMask),
                string.Format("   Ref  {0}", (int)StencilLightingUsage.NoLighting),
                "   Comp Always",
                "   Pass Replace",
                "}"
            };
        }

        public static void GetStencilStateForGBuffer(bool receiveSSR, bool useSplitLighting, ref Pass pass)
        {
            int stencilWriteMask = (int)HDRenderPipeline.StencilBitMask.LightingMask;
            int stencilRef = useSplitLighting ? (int)StencilLightingUsage.SplitLighting : (int)StencilLightingUsage.RegularLighting;

            stencilWriteMask |= (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR;
            stencilRef |= !receiveSSR ? (int)HDRenderPipeline.StencilBitMask.DoesntReceiveSSR : 0;

            stencilWriteMask |= (int)HDRenderPipeline.StencilBitMask.DecalsForwardOutputNormalBuffer;

            pass.StencilOverride = new List<string>()
            {
                "// Stencil setup",
                "Stencil",
                "{",
                string.Format("   WriteMask {0}", stencilWriteMask),
                string.Format("   Ref  {0}", stencilRef),
                "   Comp Always",
                "   Pass Replace",
                "}"
            };
        }
    }

    static class KeywordSupport
    {
        public static void DepthFirstCollectNodesFromNodeWithVariants<T>(
            List<T> nodeList,
            T node,
            KeywordSet keywordSet,
            NodeUtils.IncludeSelf includeSelf = NodeUtils.IncludeSelf.Include,
            List<int> slotIds = null
        )
            where T : AbstractMaterialNode
        {
            // no where to start
            if (node == null)
                return;

            // already added this node
            if (nodeList.Contains(node))
                return;

            var inputSlots = (node is HDPipeline.IGenerateMultiCompile multiCompileNode)
                ? multiCompileNode.GetInputSlotsFor(keywordSet.keywords)
                : node.GetInputSlots<ISlot>().Select(x => x.id);

            var ids = slotIds == null
                ? inputSlots
                : inputSlots.Where(slotIds.Contains);

            foreach (var slot in ids)
            {
                foreach (var edge in node.owner.GetEdges(node.GetSlotReference(slot)))
                {
                    if (node.owner.GetNodeFromGuid(edge.outputSlot.nodeGuid) is T outputNode)
                        DepthFirstCollectNodesFromNodeWithVariants(nodeList, outputNode, keywordSet);
                }
            }

            if (includeSelf == NodeUtils.IncludeSelf.Include)
                nodeList.Add(node);
        }

        public struct RequirementsPerVariant: IDisposable
        {
            // Keys are not owned
            readonly Dictionary<KeywordSet, ShaderGraphRequirements> m_Values;

            public IEnumerable<KeywordSet> keywordSets => m_Values.Keys;

            public ShaderGraphRequirements this[KeywordSet set] => m_Values[set];

            RequirementsPerVariant(Dictionary<KeywordSet, ShaderGraphRequirements> values)
            {
                m_Values = values;
            }

            public RequirementsPerVariant(
                in NodesPerVariant nodesPerVariant,
                AbstractMaterialNode node,
                ShaderStageCapability stageCapability,
                bool includeIntermediateSpaces
            )
            {
                m_Values = UnityEngine.Experimental.Rendering.DictionaryPool<KeywordSet, ShaderGraphRequirements>.Get();
                foreach (var pair in nodesPerVariant.iter)
                {
                    var requirements = ShaderGraphRequirements.FromNodes(pair.Value, stageCapability, includeIntermediateSpaces);

                    // Here, we directly insert the key and don't clone it, so it is not owned by this type.
                    m_Values.Add(pair.Key, requirements);
                }
            }

            public RequirementsPerVariant Union(in RequirementsPerVariant other)
            {
                var result = UnityEngine.Experimental.Rendering.DictionaryPool<KeywordSet, ShaderGraphRequirements>.Get();
                foreach (var value in m_Values)
                    result.Add(value.Key, value.Value);
                foreach (var value in other.m_Values)
                {
                    if (result.ContainsKey(value.Key))
                        result[value.Key] = result[value.Key].Union(value.Value);
                    else
                        result.Add(value.Key, value.Value);
                }

                return new RequirementsPerVariant(result);
            }

            public void Dispose()
            {
                // Don't dispose the keys of the dictionary, the keys are not owned.
                UnityEngine.Experimental.Rendering.DictionaryPool<KeywordSet, ShaderGraphRequirements>
                    .Release(m_Values);
            }
        }

        public struct NodesPerVariant: IDisposable
        {
            Dictionary<KeywordSet, List<AbstractMaterialNode>> m_NodesPerVariants;

            public static NodesPerVariant New()
            {
                return new NodesPerVariant
                {
                    m_NodesPerVariants = UnityEngine.Experimental.Rendering
                        .DictionaryPool<KeywordSet, List<AbstractMaterialNode>>.Get()
                };
            }

            public IEnumerable<KeywordSet> variants => m_NodesPerVariants.Keys;
            public IEnumerable<KeyValuePair<KeywordSet, List<AbstractMaterialNode>>> iter => m_NodesPerVariants;

            public List<AbstractMaterialNode> this[KeywordSet set] => m_NodesPerVariants[set];


            public bool AddKey(KeywordSet variant)
            {
                if (m_NodesPerVariants.ContainsKey(variant))
                    return false;

                // Clone the key to get ownership when inserting the value
                var variantCopySet = HashSetPool<string>.Get();
                foreach (var v in variant.keywords)
                    variantCopySet.Add(v);
                var variantCopy = new KeywordSet(variantCopySet);

                m_NodesPerVariants.Add(variantCopy, UnityEngine.Experimental.Rendering.ListPool<AbstractMaterialNode>.Get());
                return true;

            }

            public List<AbstractMaterialNode> GetNodeList(KeywordSet keywordSet) => m_NodesPerVariants[keywordSet];

            public void Dispose()
            {
                foreach (var pair in m_NodesPerVariants)
                {
                    pair.Key.Dispose();
                    UnityEngine.Experimental.Rendering.ListPool<AbstractMaterialNode>.Release(pair.Value);
                }
                UnityEngine.Experimental.Rendering.DictionaryPool<KeywordSet, List<AbstractMaterialNode>>.Release(m_NodesPerVariants);
            }
        }

        /// <summary> Use this string set to perform equality based on their content.</summary>
        public struct KeywordSet: IEquatable<KeywordSet>, IDisposable
        {
            readonly HashSet<string> m_Keywords;
            readonly int m_Hash;

            public HashSet<string> keywords => m_Keywords;

            public KeywordSet(HashSet<string> keywords)
            {
                Assert.IsNotNull(keywords);

                m_Keywords = keywords;
                m_Hash = 0;
                foreach (var keyword in m_Keywords)
                    m_Hash ^= keyword.GetHashCode();
            }

            public void Dispose()
            {
                HashSetPool<string>.Release(m_Keywords);
            }

            public override int GetHashCode() => m_Hash;

            public bool Equals(KeywordSet set)
                => set.m_Keywords.IsSubsetOf(m_Keywords) &&
                    set.m_Keywords.IsSupersetOf(m_Keywords);

            public override bool Equals(object obj)
                => (obj is KeywordSet set) && Equals(set);
        }

        public static void CollectNodesPerVariantsFor<T>(NodesPerVariant dst, HashSet<KeywordSet> variants, T root, List<int> slotIds = null)
            where T: AbstractMaterialNode
        {
            //
            // At this point, `dst` contains all variants that needs to be parsed.
            // Now we need to fetch the input nodes for all variants
            //

            foreach (var variant in variants)
                dst.AddKey(variant);

            foreach (var variant in variants)
            {
                var nodeList = dst.GetNodeList(variant);
                DepthFirstCollectNodesFromNodeWithVariants(nodeList, root, variant, NodeUtils.IncludeSelf.Include,
                    slotIds);
            }
        }

        public static void CollectVariantsFor<T>(HashSet<KeywordSet> allVariants, T root, List<int> slotIds)
            where T : AbstractMaterialNode
        {
            using (UnityEngine.Experimental.Rendering.ListPool<List<string>>.Get(out var multiCompileList))
            {
                //
                // Compute all variants generated by the multi compiles
                //

                using (HashSetPool<KeywordSet>.Get(out var allMultiCompiles))
                {
                    //
                    // To do so, we first need all multi compile sets that are generated by the input nodes
                    //

                    using (UnityEngine.Experimental.Rendering.ListPool<AbstractMaterialNode>.Get(out var allNodes))
                    {
                        //
                        // Search for all nodes connected to all inputs of the provided node
                        //
                        NodeUtils.DepthFirstCollectNodesFromNode(allNodes, root, NodeUtils.IncludeSelf.Include, slotIds);

                        // Now search for all nodes that can generates multi compile sets
                        foreach (var node in allNodes)
                        {
                            if (!(node is IGenerateMultiCompile multiCompileNode)) continue;

                            using (UnityEngine.Experimental.Rendering.ListPool<HashSet<string>>.Get(
                                out var multiCompiles))
                            {
                                multiCompileNode.GetMultiCompiles(HashSetPool<string>.Get, multiCompiles);
                                foreach (var multiCompileSet in multiCompiles)
                                {
                                    var newSet = new KeywordSet(multiCompileSet);
                                    if (!allMultiCompiles.Add(newSet))
                                        // We don't store this set, so we return it to the pool
                                        HashSetPool<string>.Release(multiCompileSet);
                                }
                            }
                        }
                    }

                    // `allMultiCompiles` contains all generated multi compile sets, except the default one
                    // Try to add the default multi compile
                    {
                        var defaultMultiCompile = HashSetPool<string>.Get();
                        defaultMultiCompile.Add("_");
                        var defaultSet = new KeywordSet(defaultMultiCompile);
                        if (!allMultiCompiles.Add(defaultSet))
                            // If it was not added, then release it to the pool
                            HashSetPool<string>.Release(defaultMultiCompile);
                    }

                    //
                    // At this point, we have a unique set of MultiCompileSet
                    //

                    // We can now generate all variants from the multi compiles sets

                    // But, first, convert HashSet<HashSet<string>> to List<List<string>>
                    // It is easier to iterate on List than on HashSet
                    // But still, use Pools to avoid allocations each frame
                    foreach (var multiCompileSet in allMultiCompiles)
                    {
                        if (multiCompileSet.keywords.Count == 0)
                            // Ignore empty multi compile sets
                            continue;

                        var listValues = UnityEngine.Experimental.Rendering.ListPool<string>.Get();
                        listValues.AddRange(multiCompileSet.keywords);
                        multiCompileList.Add(listValues);
                    }

                    // Release `multiCompileSet` and its values
                    foreach (var multiCompileSet in allMultiCompiles)
                        HashSetPool<string>.Release(multiCompileSet.keywords);
                }

                using (UnityEngine.Experimental.Rendering.ListPool<int>.Get(out var variantIndices))
                using (HashSetPool<string>.Get(out var variant))
                {
                    // Generate a list of indices to track which variant we are currently visiting
                    // in each sets.
                    for (var i = 0; i < multiCompileList.Count; ++i)
                        variantIndices.Add(0);

                    // Generate all variants from the multi compile
                    // Current variant combination is generated by taking the keyword from each set
                    // that is at the index defined in `variantIndices`.
                    // Then increase the indices from the end, and 'overflows' the increment to the start.
                    do
                    {
                        // There must be always at least 1 multi compile (the default one)
                        // So we are sure that the first loop needs to do some work

                        variant.Clear();
                        for (var i = 0; i < variantIndices.Count; ++i)
                        {
                            var keyword = multiCompileList[i][variantIndices[i]];
                            if (keyword != "_")
                                variant.Add(keyword);
                        }

                        // Try to add the variant
                        var set = new KeywordSet(variant);
                        if (allVariants.Add(set))
                            // The variant may already have been added, it depends on the different
                            // multi compile sets
                            //
                            // In the case the set was already added, then we need to get a new temporary HashSet
                            variant = HashSetPool<string>.Get();

                        // Increment variant combination
                        var lastIndex = variantIndices[variantIndices.Count - 1];
                        if (lastIndex < multiCompileList[variantIndices.Count - 1].Count - 1)
                            // Last set has still values to iterate on
                            variantIndices[variantIndices.Count - 1] = lastIndex + 1;
                        else
                        {
                            // We iterated on all values in the last set
                            // So we 'overflows' the increment to the previous set
                            for (var i = variantIndices.Count - 1; i >= 0; --i)
                            {
                                if (variantIndices[i] < multiCompileList[i].Count - 1)
                                {
                                    // Previous index was increased
                                    // So we have a new variant set to visit
                                    // We can break the inner loop
                                    ++variantIndices[i];
                                    break;
                                }
                                else
                                {
                                    // All values were visited in this set

                                    if (i == 0)
                                        // This was the first set, so we explored all possible
                                        // variant combination. We can break the outer loop.
                                        goto LOOP_COMPLETED;

                                    // This is an intermediate set, so we reset its index and try
                                    // to overflow the increment the previous set (next loop iteration)
                                    variantIndices[i] = 0;
                                }
                            }
                        }
                    } while (true);
                }

                LOOP_COMPLETED:

                //
                // At this point, `allVariants` contains all variants generated by the shader graph
                //

                // We don't need `multiCompileList`, so we can release it and its values as well
                foreach (var list in multiCompileList)
                    UnityEngine.Experimental.Rendering.ListPool<string>.Release(list);
            }
        }
    }
}
