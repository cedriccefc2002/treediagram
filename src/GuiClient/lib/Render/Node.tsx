import * as React from "react";

import { getLogger } from "log4js";

const logger = getLogger();

export interface ITreeEditProps {
    count: number;
}

export interface ITreeListState {
    childrenCount: number;
}

export class Node extends React.Component<ITreeEditProps, ITreeListState> {
    public constructor(props: ITreeEditProps) {
        super(props);
        this.state = {
            childrenCount: props.count,
        };
    }

    public renderChildren() {
        const rows: JSX.Element[] = [];
        const count = this.state.childrenCount - 1;
        for (let i = 0; i < this.state.childrenCount; i++) {
            rows.push(<li>
                <Node key={i} count={count} />
            </li>);
        }
        return rows;
    }
    public render() {
        return <>
            Node-{this.props.count}
            <ul>{this.renderChildren()}</ul>
        </>;
    }
}
