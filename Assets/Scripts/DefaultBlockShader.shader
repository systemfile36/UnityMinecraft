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
				//color는 정점의 색을 의미한다.
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
				
				//전역광 값
				float GlobalLightLevel;
					
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
				//v2f를 받아서 해당 위치의 텍스쳐를 조회 후 반환
				fixed4 fragFunction(v2f i) : SV_Target {
					//tex2D는 TextureLookup을 수행한다.
					//즉, 해당 위치의 텍스쳐를 조회한다.
					fixed4 col = tex2D(_MainTex, i.uv);

					//전역광 밝기에 정점의 색의 Alpha값을 더한 뒤 0 과 1사이로 clamp 한다.
					//즉, 정점 하나하나가 자신의 밝기를 가진다.
					float localLightLevel = clamp(GlobalLightLevel + i.color.a, 0, 1);

					//tex2D()를 통해 받아온 색 정보를 수정한 뒤 반환한다.
					
					//clip()은 조건에 따라 픽셀을 없애는 함수이다.
					//col.a 는 Alpha 값을 의미한다.
					//즉, Alpha값이 1 미만이면 clip()에 0미만의 값이 넘어간다.
					//그럼 픽셀을 없앤다. 
					//낭비를 막기 위해 투명한 것은 최대한 윗줄에서 버린다.
					clip(col.a - 1);
					
					//col과 RGBA(0 0 0 1)(검은색)을 선형 보간해서 반환
					//맨 마지막 인수는 weight로, 
					//이 값에 따라 검은 정도가 달라질것
					//즉, 이 부분은 그 위치의 텍스쳐 색을 weight에 따라 결정함
					//weight가 1에 가까울 수록 두번째 인자에 가까워짐
					col = lerp(col, float4(0, 0, 0, 1), localLightLevel);
					
					return col;
				}

				ENDCG
		}

	}
}