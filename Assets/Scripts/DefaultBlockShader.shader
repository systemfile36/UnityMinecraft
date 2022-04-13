//유니티의 셰이더 선택 창에서 나타날 이름
Shader "UnityMinecraft/Blocks" {

	Properties{
		//텍스쳐를 정의한다.
		//name ("display name", 2D) = "defaulttexture" {} 와 같이 정의됨
		//각 프로퍼티는 이름으로 참조됨(유니티에선 _ 로 시작되는 경우가 많음
		_MainTex ("Block Texture Atlas", 2D) = "white" {}
	}
	
	SubShader {
		//https://docs.unity3d.com/530/Documentation/Manual/SL-PassTags.html
		//키와 값의 쌍, 라이팅 파이프라인에서 가지는 역할을 제어할 때 사용
		Tags { 
			"Queue"="AlphaTest" 
			"IgnoreProjector"="True" 
			"RenderType"="TransparentCutout" 
		}
		//Level Of Detail의 약자
		LOD 100

		//빛은 따로 만들 것이므로 Off
		Lighting Off
		
		//셰이더 개체의 기본 요소,
		//그래픽 카드의 상태와 셰이딩 과정에 대한 지침(과정)을 포함함
		Pass {
			//셰이더 코드 블럭
			//유니티의 built-in shader include files를 기본 포함함
			//Built-in 렌더링 파이프라인 호환됨
			CGPROGRAM
				//필요한 함수 미리 정의
				#pragma vertex vertFunction
				#pragma fragment fragFunction		
				#pragma _target 2.0

				#include "UnityCG.cginc"
				
				//POSITION 현재 보고 있는 정점의 좌표(미리 정의됨)
				//TEXCOORD0 현재 보고 있는 픽셀의 텍스쳐의 좌표
				//			하나 이상의 텍스쳐를 쓸 수도 있으므로 뒤에 0을 붙임
				//float4는 4개의 값을 가짐, Vector3 같은 느낌으로 사용
				//float2는 역시 Vector2 같은 느낌으로 사용
				//fixed4는 아무래도 float4의 고정 소수점 버전...인가?				


				//필요한 정점과 UV 정보들이 자동으로
				//이 appdata 구조체에 들어간다.
				//color는 정점의 색을 의미한다. 이 값은 외부에서 임의로 설정한 값이다.
				struct appdata {
					
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};
				
				//appdata를 화면에 맞게 변환한 것이 담길 것
				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};
				
				//텍스쳐
				sampler2D _MainTex;
				
				//전역광 값, 셰이더 외부에서 받아온다.
				float GlobalLightLevel;

				//전역광의 최대 최소
				float minGlobalLight;
				float maxGlobalLight;
					
				//appdata를 받아서 v2f를 설정한다.
				v2f vertFunction (appdata v)
				{
					v2f o;
					
					//v2f의 vertex에 appdata를 통해 받은 World 기준 좌표를
					//스크린 좌표(카메라에 보이는)로 변환하여 넣는다.
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.color = v.color;

					return o;
				}
				
				//fixed4는 사원수 같이 4개의 값을 가짐
				//즉, RGB와 Alpha 값을 가진 컬러로 쓰임
				//v2f를 받아서 해당 위치의 픽셀의 색을
				//텍스쳐와 밝기를 고려해서 변형한 뒤 반환
				fixed4 fragFunction(v2f i) : SV_Target {

					//tex2D는 TextureLookup을 수행한다.
					//즉, 해당 위치의 텍스쳐를 조회한다.
					fixed4 col = tex2D(_MainTex, i.uv);

					//여기서 GlobalLightLevel은 밝기의 퍼센티지를 말한다.
					//1에 가까울 수록 maxGlobalLight에 일정하게 가까워 질것이고,
					//0에 가까울 수록 minGlobalLight에 일정하게 가까워 지는 것이다.
					float shade = (maxGlobalLight - minGlobalLight) * GlobalLightLevel + minGlobalLight;

					//정점의 색의 Alpha값을 곱해서 각 정점의 밝기를 반영한다.
					//각 정점의 Alpha 값에는 받는 빛의 양이 0 ~ 1로 들어간다.
					shade *= i.color.a;

					//위에서 해당 정점이 받는 빛의 양을 곱했으므로
					//반전 시켜주어야 한다. 받는 빛의 양이 많으면 그림자는 옅어져야 하기 때문
					//이렇게 반전시켜줌으로서 받는 빛의 양이 클수록, GlobalLightLevel이 클수록 밝아진다.
					shade = clamp(1 - shade, minGlobalLight, maxGlobalLight);

					//tex2D()를 통해 받아온 색 정보를 수정한 뒤 반환한다.
					//clip()은 조건에 따라 픽셀을 없애는 함수이다. col.a 는 Alpha 값을 의미한다.
					//즉, Alpha값이 1 미만이면 clip()에 0미만의 값이 넘어간다.
					//그럼 픽셀을 없앤다. 
					//낭비를 막기 위해 투명한 것은 최대한 윗줄에서 버린다.
					clip(col.a - 1);
					
					//col과 RGBA(0 0 0 1)(검은색)을 선형 보간해서 반환
					//맨 마지막 인수는 weight로, 
					//이 값에 따라 검은 정도가 달라질것
					//즉, 이 부분은 그 위치의 텍스쳐 색을 weight에 따라 결정함
					//weight가 1에 가까울 수록 두번째 인자(BLACK)에 가까워짐
					col = lerp(col, float4(0, 0, 0, 1), shade);
					
					return col;
				}

				ENDCG
		}

	}
}

/*
shade값의 의미 설명﻿
이때, 각 정점의 밝기에는 "받는 빛의 양"을 넣을 것이므로 값을 반전 시켜주는 것을 주의하라.
이 값은 받는 빛이 많을 수록 1에 가까워지는 값이므로, 빛을 받을 수록 shade 값은 1이 되어다.
하지만 이 shade 값은 커질 수록 해당 픽셀을 검은색으로 만든다. 말그대로 그림자이기 때문이다.
따라서 빛을 많이 받을수록 그림자가 옅어지게 하기위해 값을 반전시켜야 한다.
안 그러면 그림자와 밝은 부분이 반대가 된다.﻿
*/