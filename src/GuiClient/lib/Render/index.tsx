import * as React from "react";

export interface IRenderProps {
    compiler: string;
    framework: string;
}

export interface IRenderState {
    compiler: string;
    framework: string;
}

export class Render extends React.Component<IRenderProps, IRenderState> {
    public render() {
        return <h1>Hello from {this.props.compiler} and {this.props.framework}!</h1>;
    }
}
