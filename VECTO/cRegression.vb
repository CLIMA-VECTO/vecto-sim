' Copyright 2014 European Union.
' Licensed under the EUPL (the 'Licence');
'
' * You may not use this work except in compliance with the Licence.
' * You may obtain a copy of the Licence at: http://ec.europa.eu/idabc/eupl
' * Unless required by applicable law or agreed to in writing,
'   software distributed under the Licence is distributed on an "AS IS" basis,
'   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'
' See the LICENSE.txt for the specific language governing permissions and limitations.
Public Class cRegression

    Public Class RegressionProcessInfo

        Public SampleSize As Integer = 0

        Public SigmaError As Double

        Public XRangeL As Double = Double.MaxValue

        Public XRangeH As Double = Double.MinValue

        Public YRangeL As Double = Double.MaxValue

        Public YRangeH As Double = Double.MinValue

        Public StandardError As Double

        Public a As Double

        Public b As Double

        Public XStdDev As Double

        Public YStdDev As Double

        Public XMean As Double

        Public YMean As Double

        Public PearsonsR As Double

        Public t As Double

        Dim Residuals As ArrayList = New ArrayList

        Public Overrides Function ToString() As String

            Dim ret As String = "SampleSize=" & Me.SampleSize & vbCrLf & "StandardError=" & Me.StandardError & vbCrLf & "y=" & Me.a & " + " & Me.b & "x"

            Return ret

        End Function

    End Class

    Function Regress(ByVal xval() As Double, ByVal yval() As Double) As RegressionProcessInfo

        Dim sigmax As Double = 0.0

        Dim sigmay As Double = 0.0

        Dim sigmaxx As Double = 0.0

        Dim sigmayy As Double = 0.0

        Dim sigmaxy As Double = 0.0

        Dim x As Double

        Dim y As Double

        Dim n As Double = 0

        Dim ret As RegressionProcessInfo = New RegressionProcessInfo

        For arrayitem As Integer = LBound(xval) To UBound(xval)

            x = xval(arrayitem)

            y = yval(arrayitem)

            If x > ret.XRangeH Then

                ret.XRangeH = x

            End If

            If x < ret.XRangeL Then

                ret.XRangeL = x

            End If

            If y > ret.YRangeH Then

                ret.YRangeH = y

            End If

            If y < ret.YRangeL Then

                ret.YRangeL = y

            End If

            sigmax += x

            sigmaxx += x * x

            sigmay += y

            sigmayy += y * y

            sigmaxy += x * y

            n = n + 1

        Next

        ret.b = (n * sigmaxy - sigmax * sigmay) / (n * sigmaxx - sigmax * sigmax)

        ret.a = (sigmay - ret.b * sigmax) / n

        ret.SampleSize = CType(n, Integer)

        'calculate distances for each point (residual)

        For arr2 As Integer = LBound(xval) To UBound(xval)

            y = yval(arr2)

            x = xval(arr2)

            Dim yprime As Double = ret.a + ret.b * x 'prediction

            Dim Residual As Double = y - yprime

            ret.SigmaError += Residual * Residual

        Next

        ret.XMean = sigmax / n

        ret.YMean = sigmay / n

        ret.XStdDev = Math.Sqrt((CType(n * sigmaxx - sigmax * sigmax, Double)) / (CDbl(n) * CDbl(n) - 1.0))

        ret.YStdDev = Math.Sqrt((CType(n * sigmayy - sigmay * sigmay, Double)) / (CDbl(n) * CDbl(n) - 1.0))

        ret.StandardError = Math.Sqrt(ret.SigmaError / ret.SampleSize)

        Dim ssx As Double = sigmaxx - ((sigmax * sigmax) / n)

        Dim ssy As Double = sigmayy - ((sigmay * sigmay) / n)

        Dim ssxy As Double = sigmaxy - ((sigmax * sigmay) / n)

        ret.PearsonsR = ssxy / Math.Sqrt(ssx * ssy)

        ret.t = ret.PearsonsR / Math.Sqrt((1 - (ret.PearsonsR * ret.PearsonsR)) / (n - 2))

        Return ret

    End Function

End Class
